import { AppComponentBase } from '@shared/app-component-base';
import { Component, OnInit, Injector } from '@angular/core';

@Component({
  selector: 'app-tai-san',
  templateUrl: './tai-san.component.html',
  styleUrls: ['./tai-san.component.scss']
})
export class TaiSanComponent extends AppComponentBase implements OnInit {

  constructor(injector: Injector) {
    super(injector);
  }

  ngOnInit(): void {
  }

  onTabChange(event) {
    abp.utils.setCookieValue(
      'taiSanTabIndexActive',
      event.index,
      new Date(this.today.setDate(this.today.getDate() + 1)),
      abp.appPath
    );
  }

}
