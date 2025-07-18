import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection } from "@angular/core"
import { provideRouter } from "@angular/router"
import { HttpClient, HttpClientModule, provideHttpClient, withInterceptors } from "@angular/common/http"
import { provideAnimations } from "@angular/platform-browser/animations" // For BrowserAnimationsModule
import { ToastaModule } from "ngx-toasta" // For ToastaModule.forRoot()

import { routes } from "./app.routes" // Changed from app-routing.module to app.routes
import { ErrorInterceptor } from "./interceptors/error.interceptor"
import { AuthService } from "./services/auth.service"
import { ApiService } from "./services/api.service"
import { UserService } from "./services/user.service"
import { MessageService } from "./services/message.service"
import { JobService } from "./services/job.service"
import { GroupService } from "./services/group.service"
import { InstanceService } from "./services/instance.service"

import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes), // Cung cấp router với các routes đã định nghĩa
    provideHttpClient(withInterceptors([ErrorInterceptor])), // Cung cấp HttpClient với interceptor
    provideAnimations(), // Cung cấp animations
    ToastaModule.forRoot().providers!, // Cung cấp ToastaModule
    // Cung cấp các services ở cấp độ root nếu chúng chưa có providedIn: 'root'
    // Hầu hết các services nên có providedIn: 'root' để tối ưu tree-shaking
    AuthService,
    ApiService,
    UserService,
    MessageService,
    JobService,
    GroupService,
    InstanceService,
    importProvidersFrom(
      TranslateModule.forRoot({
        defaultLanguage: 'en',
        loader: {
          provide: TranslateLoader,
          useFactory: HttpLoaderFactory,
          deps: [HttpClient]
        }
      })
    )
  ],
}
