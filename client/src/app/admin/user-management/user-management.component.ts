import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {

  users: User[] = [];
  bsModalRef: BsModalRef<RolesModalComponent> = new BsModalRef<RolesModalComponent>();
  availableRoles: string[] = ['Admin', 'Moderator', 'Member'];

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe({
      next: users => {
        this.users = users;
      }
    })
  }

  openRolesModal(user : User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        userName : user.userName,
        availableRoles : this.availableRoles,
        selectedRoles : [...user.roles]
      }
    }
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.onHide?.subscribe({
      next: () => {
        let selectedRoles = ['Member'];
        if (this.bsModalRef.content) {
          selectedRoles = this.bsModalRef.content?.selectedRoles;
        }
        if (!this.arrayEquals(selectedRoles, user.roles)) {
          this.adminService.updateUserRoles(user.userName, selectedRoles.join(',')).subscribe({
            next: roles => {
              user.roles = roles;
            }
          })
        }
      }
    })
  }

  private arrayEquals(a: any[], b: any[]) {
    return JSON.stringify(a.sort()) === JSON.stringify(b.sort());
  }
}
