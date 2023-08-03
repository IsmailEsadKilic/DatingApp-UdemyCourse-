using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            
        }
        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(m => m.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUserName == messageParams.UserName
                                            && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.SenderUserName == messageParams.UserName
                                             && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipientUserName == messageParams.UserName
                                      && u.RecipientDeleted == false && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserUserName, string recipientUserName)
        {
            var messages = await _context.Messages
                .Include (u => u.Sender).ThenInclude(p => p.Photos)
                .Include (u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(
                    m => (m.RecipientUserName == currentUserUserName && m.RecipientDeleted == false
                         && m.SenderUserName == recipientUserName)
                         || (m.RecipientUserName == recipientUserName
                         && m.SenderUserName == currentUserUserName && m.SenderDeleted == false)
                ).OrderBy(m => m.MessageSent)
                .ToListAsync();
            
            var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUserName == currentUserUserName).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = System.DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);  
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}