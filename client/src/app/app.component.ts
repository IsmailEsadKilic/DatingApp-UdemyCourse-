import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { AccountService } from './_services/account.service';
import { User } from './_models/user';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  
  title = 'Dating app';

  constructor(private accountService: AccountService) { }

  ngOnInit(): void {
    //there used to be a this.getUsers() here
    this.setCurrentUser();
  }

  setCurrentUser() {
    const userstring = localStorage.getItem('user');
    if (userstring) {
      const user: User = JSON.parse(userstring);
      this.accountService.setCurrentUser(user);
    }
  }
}         