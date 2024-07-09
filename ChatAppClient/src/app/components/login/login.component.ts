import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  name: string = '';

  constructor(private http: HttpClient, private router: Router) { }

  login() {
    this.http.get("https://localhost:44389/api/Auth/Login?name=" + this.name).subscribe({
      next: res => {
        localStorage.setItem('accessToken', JSON.stringify(res));
        this.router.navigateByUrl('/');
      },
      error: (err: HttpErrorResponse) => {
        console.log(err);
      }
    })
  }
}
