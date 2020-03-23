import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { NgZorroAntdModule } from 'ng-zorro-antd';
import { PagesRoutingModule } from './pages-routing.module';
import { AppListComponent } from './app-list/app-list.component';
import { UploadPackageComponent } from './upload-package/upload-package.component';
import { SystemSettingComponent } from './system-setting/system-setting.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppEditComponent } from './app-edit/app-edit.component';
import { WebAppListComponent } from './web-app-list/web-app-list.component';
import { WebAppEditComponent } from './web-app-edit/web-app-edit.component';


@NgModule({
  declarations: [
    AppListComponent,
    UploadPackageComponent,
    SystemSettingComponent,
    AppEditComponent,
    WebAppListComponent,
    WebAppEditComponent],
  imports: [
    CommonModule,
    PagesRoutingModule,
    NgZorroAntdModule,
    FormsModule,
    ReactiveFormsModule
  ]
})
export class PagesModule { }