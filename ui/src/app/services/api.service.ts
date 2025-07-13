import { Injectable } from "@angular/core"
import { HttpClient, HttpHeaders } from "@angular/common/http"
import { Observable } from "rxjs"
import { AuthService } from "./auth.service"

@Injectable({
  providedIn: "root",
})
export class ApiService {
  constructor(
    private http: HttpClient,
    private authService: AuthService,
  ) {}

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken()
    return new HttpHeaders({
      "Content-Type": "application/json",
      ...(token && { Authorization: `Bearer ${token}` }),
    })
  }

  get<T>(url: string): Observable<T> {
    return this.http.get<T>(url, { headers: this.getHeaders() })
  }

  post<T>(url: string, body: any): Observable<T> {
    return this.http.post<T>(url, body, { headers: this.getHeaders() })
  }

  put<T>(url: string, body: any): Observable<T> {
    return this.http.put<T>(url, body, { headers: this.getHeaders() })
  }

  delete<T>(url: string): Observable<T> {
    return this.http.delete<T>(url, { headers: this.getHeaders() })
  }

  postFile<T>(url: string, formData: FormData): Observable<T> {
    const token = this.authService.getToken()
    const headers = new HttpHeaders({
      ...(token && { Authorization: `Bearer ${token}` }),
    })
    // Do NOT set 'Content-Type': 'multipart/form-data' here.
    // The browser will set it automatically with the correct boundary.
    return this.http.post<T>(url, formData, { headers })
  }

  putFile<T>(url: string, formData: FormData): Observable<T> {
    const token = this.authService.getToken()
    const headers = new HttpHeaders({
      ...(token && { Authorization: `Bearer ${token}` }),
    })
    return this.http.put<T>(url, formData, { headers })
  }
}
