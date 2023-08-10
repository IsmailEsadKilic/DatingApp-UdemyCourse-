using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public PhotoRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            
        }
        public async Task<Photo> GetPhotoByIdAsync(int id)
        {
            return await _context.Photos
                .IgnoreQueryFilters() //ignore the query filters
                .Where(p => p.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<PhotoApprovalDto>> GetPhotosForApprovalAsync()
        {
            return await _context.Photos
                .IgnoreQueryFilters()
                .Where(p => p.IsApproved == false)
                .ProjectTo<PhotoApprovalDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public void RemovePhoto(Photo photo) //doesnt remove from cloudinary
        {
            if (photo != null) _context.Photos.Remove(photo);
        }
    }
}