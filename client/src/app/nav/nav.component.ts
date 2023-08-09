import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { User } from '../_models/user';
import { Observable, of } from 'rxjs';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  constructor(public accountService: AccountService, private router: Router,
    private toastr: ToastrService) { }

  ngOnInit(): void {
  }

  Login() {
    this.accountService.login(this.model).subscribe({
      next: () => {this.router.navigateByUrl('/members'), this.toastr.success('Logged in successfully'),
        this.model = {}
      }
    })
  }

  Logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
