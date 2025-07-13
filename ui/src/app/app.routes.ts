import { Routes } from "@angular/router"

// Guards
import { AuthGuard } from "./guards/auth.guard"
import { AdminGuard } from "./guards/admin.guard"

// Components (Standalone components)
import { LoginComponent } from "./auth/login/login.component"
import { DashboardComponent } from "./dashboard/dashboard.component"
import { MessagesComponent } from "./messages/messages.component"
import { MessageFormComponent } from "./messages/message-form/message-form.component"
import { JobsComponent } from "./jobs/jobs.component"
import { JobFormComponent } from "./jobs/job-form/job-form.component"
import { JobDetailComponent } from "./jobs/job-detail/job-detail.component"
import { GroupsComponent } from "./groups/groups.component"
import { InstancesComponent } from "./instances/instances.component"
import { InstanceFormComponent } from "./instances/instance-form/instance-form.component"
import { UsersComponent } from "./users/users.component"
import { UserFormComponent } from "./users/user-form/user-form.component"

export const routes: Routes = [
  { path: "", redirectTo: "/dashboard", pathMatch: "full" },
  { path: "login", component: LoginComponent },
  {
    path: "dashboard",
    component: DashboardComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "messages",
    component: MessagesComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "messages/new",
    component: MessageFormComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "messages/edit/:id",
    component: MessageFormComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "jobs",
    component: JobsComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "jobs/new",
    component: JobFormComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "jobs/:id",
    component: JobDetailComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "groups",
    component: GroupsComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "instances",
    component: InstancesComponent,
    canActivate: [AuthGuard, AdminGuard],
  },
  {
    path: "instances/new",
    component: InstanceFormComponent,
    canActivate: [AuthGuard, AdminGuard],
  },
  {
    path: "instances/edit/:id",
    component: InstanceFormComponent,
    canActivate: [AuthGuard, AdminGuard],
  },
  {
    path: "users",
    component: UsersComponent,
    canActivate: [AuthGuard, AdminGuard],
  },
  {
    path: "users/new",
    component: UserFormComponent,
    canActivate: [AuthGuard, AdminGuard],
  },
  {
    path: "users/edit/:id",
    component: UserFormComponent,
    canActivate: [AuthGuard, AdminGuard],
  },
  { path: "**", redirectTo: "/dashboard" },
]
