import { Injectable } from '@angular/core';
import { BasiceService } from './BasiceService';
import { Router } from '@angular/router';
import { HttpClient, HttpRequest, HttpEvent, HttpEventType, HttpResponse, HttpHeaders } from '@angular/common/http';
import { NzMessageService, UploadXHRArgs } from 'ng-zorro-antd';
import { AuthorityCommon } from '../components/authority-common';
import { ResultDataModel } from './models/result/resultDataModel';
import { AppListModel } from './models/app/AppListModel';
import { AppModel } from './models/app/appModel';
import { ResultModel } from './models/result/resultModel';
import { WebAppModel } from './models/app/WebAppModel';

@Injectable({
  providedIn: 'root'
})
export class AppService extends BasiceService {
  constructor(protected route: Router, protected http: HttpClient, protected message: NzMessageService,
              protected authorityCommon: AuthorityCommon) {
    super(route, http, message, authorityCommon);
  }
  /**
   * 获得应用列表
   */
  public getAppList(success?: (value: ResultDataModel<AppListModel[]>) => void, complete?: () => void) {
    return this.sendGet('/App/GetAppList', null, success, null, complete);
  }
  /**
   * 获得应用信息
   */
  public getAppInfo(id: string, success?: (value: ResultDataModel<AppModel>) => void, complete?: () => void) {
    return this.sendGet('/App/GetAppInfo', { id }, success, null, complete);
  }
  /**
   * 添加一个应用
   */
  public addApp(data: AppModel, success?: (value: ResultModel) => void, complete?: () => void) {
    return this.sendPost('/App/AddApp', data, success, null, complete);
  }
  /**
   * 修改一个应用
   */
  public editApp(data: AppModel, success?: (value: ResultModel) => void, complete?: () => void) {
    return this.sendPost('/App/EditApp', data, success, null, complete);
  }
  /**
   * 删除一个应用
   */
  public deleteApp(id: string, success?: (value: ResultModel) => void, complete?: () => void) {
    return this.sendGet('/App/DeleteApp', { id }, success, null, complete);
  }
  /**
   * 获得Web应用列表
   */
  public getWebAppList(success?: (value: ResultDataModel<WebAppModel[]>) => void, complete?: () => void) {
    return this.sendGet('/App/GetWebAppList', null, success, null, complete);
  }
  /**
   * 获得Web应用信息
   */
  public getWebAppInfo(id: string, success?: (value: ResultDataModel<WebAppModel>) => void, complete?: () => void) {
    return this.sendGet('/App/GetWebAppInfo', { id }, success, null, complete);
  }
  /**
   * 添加一个Web应用
   */
  public addWebApp(data: WebAppModel, success?: (value: ResultModel) => void, complete?: () => void) {
    return this.sendPost('/App/AddWebApp', data, success, null, complete);
  }
  /**
   * 修改一个Web应用
   */
  public editWebApp(data: WebAppModel, success?: (value: ResultModel) => void, complete?: () => void) {
    return this.sendPost('/App/EditWebApp', data, success, null, complete);
  }
  /**
   * 删除一个Web应用
   */
  public deleteWebApp(id: string, success?: (value: ResultModel) => void, complete?: () => void) {
    return this.sendGet('/App/DeleteWebApp', { id }, success, null, complete);
  }
  /**
   * 启动所有应用
   */
  public startAllApp(success?: (value: ResultModel) => void, complete?: () => void) {
    return this.sendGet('/App/StartAllApp', null, success, null, complete);
  }
  /**
   * 停止所有应用
   */
  public stopAllApp(success?: (value: ResultModel) => void, complete?: () => void) {
    return this.sendGet('/App/StopAllApp', null, success, null, complete);
  }
  /**
   * 启动应用
   */
  public startApp(id: string, success?: (value: ResultModel) => void, complete?: () => void) {
    return this.sendGet('/App/StartApp', { id }, success, null, complete);
  }
  /**
   * 停止应用
   */
  public stopApp(id: string, success?: (value: ResultModel) => void, complete?: () => void) {
    return this.sendGet('/App/StopApp', { id }, success, null, complete);
  }
  /**
   * 获得控制台列表
   */
  public getConsoleList(id: string, success?: (value: ResultDataModel<string[]>) => void, complete?: () => void) {
    return this.sendGet('/App/GetConsoleList', { id }, success, null, complete);
  }
  /**
   * 上传文件
   */
  public updateAppFile(item: UploadXHRArgs) {
    const url = `${this.baseUrl}/App/UpdateAppFile`;
    const formData = new FormData();
    formData.append('file', item.file as any);
    const initParams: any = {
      reportProgress: true,
      withCredentials: true
    };
    const token = this.authorityCommon.getToken();
    if (token) {
      initParams.headers = new HttpHeaders({ Authorization: token });
    }
    const req = new HttpRequest('POST', url, formData, initParams);
    return this.http.request(req).subscribe(
      (event: HttpEvent<{}>) => {
        if (event.type === HttpEventType.UploadProgress) {
          if (event.total > 0) {
            (event as any).percent = (event.loaded / event.total) * 100;
          }
          item.onProgress(event, item.file);
        } else if (event instanceof HttpResponse) {
          item.onSuccess(event.body, item.file, event);
        }
      },
      error => {
        item.onError(error, item.file);
        this.handlerError(error);
      }
    );
  }
}
