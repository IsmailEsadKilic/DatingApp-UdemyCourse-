import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';
import { ConfirmService } from '../_services/confirm.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<MemberEditComponent> {
  constructor(private confirmService: ConfirmService) { }
  canDeactivate(component: MemberEditComponent): Observable<boolean> | boolean{
    if (component.editForm?.dirty) {
      return this.confirmService.confirm()  
    }
    return true;
  }
} 
