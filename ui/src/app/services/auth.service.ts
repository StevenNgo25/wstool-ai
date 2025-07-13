import { Injectable } from "@angular/core"
import { HttpClient } from "@angular/common/http"
import { BehaviorSubject, Observable, tap } from "rxjs"
import { environment } from "../../environments/environment"
import { LoginRequest, LoginResponse } from "../models/user.model"
import { User } from "../models/user.model"
import { Router } from "@angular/router"

@Injectable({
  providedIn: "root",
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<User | null>
  public currentUser$: Observable<User | null>

  constructor(
    private http: HttpClient,
    public router: Router,
  ) {
    this.currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromLocalStorage())
    this.currentUser$ = this.currentUserSubject.asObservable()
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, credentials).pipe(
      tap((response) => {
        localStorage.setItem("token", response.token)
        localStorage.setItem("user", JSON.stringify(response.user))
        this.currentUserSubject.next(response.user)
      }),
    )
  }

  logout(): void {
    localStorage.removeItem("token")
    localStorage.removeItem("user")
    this.currentUserSubject.next(null)
    this.router.navigate(["/login"])
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem("token") && !!this.currentUserSubject.value
  }

  isAdmin(): boolean {
    const user = this.currentUserSubject.value
    return user?.role === "Admin"
  }

  getToken(): string | null {
    return localStorage.getItem("token")
  }

  private getUserFromLocalStorage(): User | null {
    const userString = localStorage.getItem("user")
    return userString ? JSON.parse(userString) : null
  }
}
