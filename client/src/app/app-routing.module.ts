import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { ListsComponent } from './lists/lists.component';
import { MessagesComponent } from './messages/messages.component';
import { AuthGuard } from './_guards/auth.guard';

const routes: Routes = [
  {path: '', component: HomeComponent}, // localhost:4200
  {path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    children: [
      {path: 'members', component: MemberListComponent}, // localhost:4200/members
      {path: 'members/:id', component: MemberDetailComponent}, // localhost:4200/members/3
      {path: 'lists', component: ListsComponent}, // localhost:4200/lists
      {path: 'messages', component: MessagesComponent}, // localhost:4200/messages
    ]
  },
  {path: '**', component: HomeComponent, pathMatch: 'full'} // localhost:4200/whatever
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
