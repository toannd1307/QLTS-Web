 // tslint:disable
 import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { distinctUntilChanged, pluck } from 'rxjs/operators';
import { LayoutConfig } from './layout-config';

@Injectable()
export class LayoutStoreService {
  public readonly config$: Observable<LayoutConfig>;
  private readonly initialLayoutConfig: LayoutConfig = {
    sidebarExpanded: true,
    childAppsExpanded: true,
  };
  private configSource: BehaviorSubject<LayoutConfig>;

  constructor() {
    this.configSource = new BehaviorSubject(this.initialLayoutConfig);
    this.config$ = this.configSource.asObservable();
  }

  get sidebarExpanded(): Observable<boolean> {
    return this.config$.pipe(
      pluck('sidebarExpanded'),
      distinctUntilChanged()
    ) as Observable<boolean>;
  }

  get childAppsExpanded(): Observable<boolean> {
    return this.config$.pipe(
      pluck('childAppsExpanded'),
      distinctUntilChanged()
    ) as Observable<boolean>;
  }

  public setSidebarExpanded(value: boolean): void {
    this.configSource.next(
      Object.assign(this.configSource.value, { sidebarExpanded: value })
    );
  }

  public setChildAppsExpanded(value: boolean): void {
    this.configSource.next(
      Object.assign(this.configSource.value, { childAppsExpanded: value })
    );
  }
}
