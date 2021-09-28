import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderComponent {
  slides = [
    {'image': './assets//img/stock-photo-asset-management-business-technology-internet-concept-button-on-virtual-screen-1368981482.jpeg'}, 
    {'image': './assets//img/asset-management.png'},
    {'image': './assets//img/IT-Asset-Management.jpg'},
  ];
}
