import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UserModel } from '../../models/user.model';
import { ChatModel } from '../../models/chat.model';
import * as signalR from '@microsoft/signalr';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterOutlet, CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  users: UserModel[] = [];
  chats: ChatModel[] = [];
  selectedUserId: string = "";
  selectedUser: UserModel = new UserModel();
  user = new UserModel();
  hub: signalR.HubConnection | undefined;
  message: string = '';

  constructor(
    private http: HttpClient
  ) {
    this.user = JSON.parse(localStorage.getItem("accessToken") ?? "");
    this.getUsers();

    this.hub = new signalR.HubConnectionBuilder().withUrl("https://localhost:44389/chat-hub").build();

    this.hub.start().then(() => {
      console.log("Connection is started...");

      this.hub?.invoke("Connect", this.user.id);

      this.hub?.on("Users", (res: UserModel) => {
        console.log(res);
        this.users.find(p => p.id == res.id)!.status = res.status;
      });

      this.hub?.on("Messages", (res: ChatModel) => {
        console.log(res);

        if (this.selectedUserId == res.toUserId) {
          this.chats.push(res);
        }
      })
    })
  }

  getUsers() {
    this.http.get<UserModel[]>("https://localhost:44389/api/Chats/GetUsers").subscribe({
      next: res => {
        this.users = res.filter(p => p.id != this.user.id)
      },
      error: (err: HttpErrorResponse) => {
        console.log(err);

      }
    })
  }

  changeUser(user: UserModel) {
    this.selectedUserId = user.id;
    this.selectedUser = user;

    this.http.get(`https://localhost:44389/api/Chats/GetChats?fromUserId=${this.user.id}&toUserId=${this.selectedUserId}`).subscribe({
      next: (res: any) => {
        this.chats = res;
      },
      error: (err: HttpErrorResponse) => {
        console.log(err);
      }
    })
  }

  logout() {
    localStorage.clear();
    document.location.reload();
  }

  sendMessage() {
    const data = {
      fromUserId: this.user.id,
      toUserId: this.selectedUserId,
      message: this.message
    };

    this.http.post<ChatModel>('https://localhost:44389/api/Chats/SendMessage', data).subscribe({
      next: res => {
        console.log(res);
        this.chats.push(res);
        this.message = '';
      },
      error: (err: HttpErrorResponse) => {
        console.log(err);
      }
    })
  }

}
