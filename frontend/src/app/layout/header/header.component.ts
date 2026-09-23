import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {

  constructor(
    public authService: AuthService
  ) {
  }

  logout(): void {
    this.authService.logout();
  }

  getInitials(): string {
    const user = this.authService.currentUser();

    if (!user) {
      return '';
    }

    return (
      user.firstName.charAt(0) +
      user.lastName.charAt(0)
    ).toUpperCase();
  }
}
