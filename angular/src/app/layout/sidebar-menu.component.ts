// tslint:disable
import { Component, Injector, OnInit } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import {
  Router,
  RouterEvent,
  NavigationEnd,
  PRIMARY_OUTLET
} from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { filter } from 'rxjs/operators';
import { MenuItem } from '@shared/layout/menu-item';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { MegaMenuItem } from 'primeng/api';

@Component({
  selector: 'sidebar-menu',
  templateUrl: './sidebar-menu.component.html',
})
export class SidebarMenuComponent extends AppComponentBase implements OnInit {
  menuItems: MenuItem[];
  menuItemsMap: { [key: number]: MenuItem } = {};
  activatedMenuItems: MenuItem[] = [];
  routerEvents: BehaviorSubject<RouterEvent> = new BehaviorSubject(undefined);
  homeRoute = '/app/home';
  testmenu = false;
  items: MegaMenuItem[];

  constructor(injector: Injector,
    private router: Router,
    public breakpointObserver: BreakpointObserver) {
    super(injector);
    this.router.events.subscribe(this.routerEvents);
  }

  ngOnInit(): void {
    this.items = [
      {
        label: 'Quản lý Tài sản',
        icon: 'fas fa-cubes',
        routerLink: '/app/main/tai-san',
        visible: this.isGranted('Pages.QuanLyTaiSan')
      },
      {
        label: 'Quản lý Đầu đọc',
        icon: 'fas fa-hdd',
        items: [
          [
            {
              label: 'Quản lý Đầu đọc',
              items: [
                {
                  label: 'Quản lý Đầu đọc di động',
                  routerLink: '/app/main/dau-doc-the-rfid',
                  visible: this.isGranted('Pages.QuanLyDauDocDiDong')
                },
                {
                  label: 'Quản lý Đầu đọc cố định',
                  routerLink: '/app/main/angten-rfid/dau-doc-co-dinh',
                  visible: this.isGranted('Pages.QuanLyDauDocCoDinh')
                },
              ]
            }]
        ],
        visible: this.isGrantedAny('Pages.QuanLyDauDocDiDong', 'Pages.QuanLyDauDocCoDinh')
      },
      {
        label: 'Giám sát Tài sản',
        icon: 'far fa-window-restore',
        items: [
          [
            {
              label: 'Giám sát Tài sản',
              items: [
                {
                  label: 'Giám sát Tài sản',
                  routerLink: '/app/main/angten-rfid/giam-sat-tai-san',
                  visible: this.isGranted('Pages.GiamSatTaiSan')
                },
                {
                  label: 'Theo dõi kết nối thiết bị',
                  routerLink: '/app/main/angten-rfid/theo-doi-ket-noi-thiet-bi',
                  visible: this.isGranted('Pages.TheoDoiKetNoiThietBi')
                },
              ]
            }]
        ],
        visible: this.isGrantedAny('Pages.GiamSatTaiSan', 'Pages.TheoDoiKetNoiThietBi')
      },
      {
        label: 'Quản lý Kiểm kê tài sản',
        icon: 'fas fa-tasks',
        routerLink: '/app/main/kiem-ke-tai-san',
        visible: this.isGranted('Pages.QuanLyKiemKeTaiSan')
      },
      {
        label: 'Quản lý Dự trù mua sắm',
        icon: 'fas fa-shopping-cart',
        routerLink: '/app/main/tai-san/du-tru-mua-sam',
        visible: this.isGranted('Pages.QuanLyDuTruMuaSam')
      },
      {
        label: 'Quản lý Cảnh báo',
        icon: 'fas fa-bell',
        routerLink: '/app/main/danh-sach-canh-bao',
        visible: this.isGranted('Pages.QuanLyCanhBao')
      },
      {
        label: 'Báo cáo', icon: 'fas fa-file-alt',
        items: [
          [
            {
              label: 'Báo cáo',
              items: [
                { label: 'Báo cáo người dùng', routerLink: '/app/main/bao-cao/bao-cao-nguoi-dung', visible: this.isGranted('Pages.BaoCaoNguoiDung') },
                { label: 'Đặt lịch xuất báo cáo', routerLink: '/app/main/bao-cao/dat-lich-xuat-bao-cao', visible: this.isGranted('Pages.DatLichXuatBaoCao') },
                { label: 'Báo cáo cảnh báo', routerLink: '/app/main/bao-cao/bao-cao-canh-bao', visible: this.isGranted('Pages.BaoCaoCanhBao') },
                { label: 'Báo cáo thông tin thiết bị RFID', routerLink: '/app/main/bao-cao/bao-cao-thong-tin-thiet-bi-rfid', visible: this.isGranted('Pages.BaoCaoThongTinThietBiRFID') },
                { label: 'Báo cáo thông tin tài sản', routerLink: '/app/main/bao-cao/bao-cao-thong-tin-tai-san', visible: this.isGranted('Pages.BaoCaoThongTinTaiSan') },
              ]
            }
          ]
        ],
        visible: this.isGrantedAny('Pages.BaoCaoNguoiDung', 'Pages.DatLichXuatBaoCao', 'Pages.BaoCaoCanhBao', 'Pages.BaoCaoThongTinThietBiRFID', 'Pages.BaoCaoThongTinTaiSan')
      },
      {
        label: 'Quản lý Danh mục',
        icon: 'fas fa-th-large',
        items: [
          [
            {
              label: 'Quản lý Danh mục',
              items: [
                {
                  label: 'Quản lý Nhà cung cấp',
                  routerLink: '/app/main/nha-cung-cap',
                  visible: this.isGranted('Pages.QuanLyNhaCungCap')
                },
                {
                  label: 'Quản lý Vị trí địa lý',
                  routerLink: '/app/main/vi-tri-dia-ly',
                  visible: this.isGranted('Pages.QuanLyViTriDiaLy')
                },
                {
                  label: 'Quản lý Loại tài sản',
                  routerLink: '/app/main/loai-tai-san',
                  visible: this.isGranted('Pages.QuanLyLoaiTaiSan')
                },
                {
                  label: 'Quản lý Đơn vị',
                  routerLink: '/app/main/to-chuc',
                  visible: this.isGranted('Pages.QuanLyPhongBan')
                },
              ]
            }]
        ],
        visible: this.isGrantedAny('Pages.QuanLyNhaCungCap', 'Pages.QuanLyViTriDiaLy', 'Pages.QuanLyLoaiTaiSan', 'Pages.QuanLyPhongBan')
      },
      {
        label: 'Quản lý Hệ thống', icon: 'fas fa-users',
        items: [
          [
            {
              label: 'Quản lý Hệ thống',
              items: [
                {
                  label: 'Quản lý Người dùng',
                  routerLink: '/app/users',
                  visible: this.isGranted('Pages.Users')
                },
                {
                  label: 'Quản lý Phân quyền',
                  routerLink: '/app/roles',
                  visible: this.isGranted('Pages.Roles')
                },
                {
                  label: 'Lịch sử Người dùng',
                  routerLink: '/app/auditLogs',
                  visible: this.isGranted('Pages.QuanLyLichSuNguoiDung')
                },
                {
                  label: 'Quản lý Mail Server',
                  routerLink: '/app/main/cau-hinh-mail-server',
                  visible: this.isGranted('Pages.QuanLyMailServer')
                },
              ]
            }
          ]
        ],
        visible: this.isGrantedAny('Pages.Users', 'Pages.Roles', 'Pages.QuanLyLichSuNguoiDung', 'Pages.QuanLyMailServer')
      },
    ];

    this.menuItems = this.getMenuItems();
    this.patchMenuItems(this.menuItems);
    this.routerEvents
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event) => {
        const currentUrl = event.url !== '/' ? event.url : this.homeRoute;
        const primaryUrlSegmentGroup = this.router.parseUrl(currentUrl).root
          .children[PRIMARY_OUTLET];
        if (primaryUrlSegmentGroup) {
          this.activateMenuItems('/' + primaryUrlSegmentGroup.toString());
        }
      });

    this.breakpointObserver
      .observe([
        Breakpoints.XSmall,
        Breakpoints.Small,
        Breakpoints.Medium,
        Breakpoints.Large,
        Breakpoints.XLarge,
      ])
      .subscribe((state: BreakpointState) => {
        if (state.breakpoints[Breakpoints.XSmall] || state.breakpoints[Breakpoints.Small]) {
          this.testmenu = false;
        }
        else if (state.breakpoints[Breakpoints.Medium] || state.breakpoints[Breakpoints.Large] || state.breakpoints[Breakpoints.XLarge]) {
          this.testmenu = true;
        }
      });
  }

  getMenuItems(): MenuItem[] {
    return [
      new MenuItem('Quản lý Tài sản', '/app/main/tai-san', 'fas fa-cubes', 'Pages.QuanLyTaiSan'),
      new MenuItem('Quản lý Đầu đọc', '', 'fas fa-hdd', this.getPermission('Pages.QuanLyDauDocDiDong', 'Pages.QuanLyDauDocCoDinh'),
        [
          new MenuItem('Quản lý Đầu đọc di động', '/app/main/dau-doc-the-rfid', '', 'Pages.QuanLyDauDocDiDong'),
          new MenuItem('Quản lý Đầu đọc cố định', '/app/main/angten-rfid/dau-doc-co-dinh', '', 'Pages.QuanLyDauDocCoDinh'),
        ]),
      new MenuItem('Giám sát Tài sản', '', 'far fa-window-restore', this.getPermission('Pages.GiamSatTaiSan', 'Pages.TheoDoiKetNoiThietBi'),
        [
          new MenuItem('Giám sát Tài sản', '/app/main/angten-rfid/giam-sat-tai-san', '', 'Pages.GiamSatTaiSan'),
          new MenuItem('Theo dõi kết nối thiết bị', '/app/main/angten-rfid/theo-doi-ket-noi-thiet-bi', '', 'Pages.TheoDoiKetNoiThietBi'),
        ]),
      new MenuItem('Quản lý Kiểm kê tài sản', '/app/main/kiem-ke-tai-san', 'fas fa-tasks', 'Pages.QuanLyKiemKeTaiSan'),
      new MenuItem('Quản lý Dự trù mua sắm', '/app/main/tai-san/du-tru-mua-sam', 'fas fa-shopping-cart', 'Pages.QuanLyDuTruMuaSam'),
      new MenuItem('Quản lý Cảnh báo', '/app/main/danh-sach-canh-bao', 'fas fa-bell', 'Pages.QuanLyCanhBao'),
      new MenuItem('Báo cáo', '', 'fas fa-file-alt',
        this.getPermission('Pages.BaoCaoNguoiDung', 'Pages.DatLichXuatBaoCao', 'Pages.BaoCaoCanhBao', 'Pages.BaoCaoThongTinThietBiRFID', 'Pages.BaoCaoThongTinTaiSan'),
        [
          new MenuItem('Báo cáo người dùng', '/app/main/bao-cao/bao-cao-nguoi-dung', '', 'Pages.BaoCaoNguoiDung'),
          new MenuItem('Đặt lịch xuất báo cáo', '/app/main/bao-cao/dat-lich-xuat-bao-cao', '', 'Pages.DatLichXuatBaoCao'),
          new MenuItem('Báo cáo cảnh báo', '/app/main/bao-cao/bao-cao-canh-bao', '', 'Pages.BaoCaoCanhBao'),
          new MenuItem('Báo cáo thông tin thiết bị RFID', '/app/main/bao-cao/bao-cao-thong-tin-thiet-bi-rfid', '', 'Pages.BaoCaoThongTinThietBiRFID'),
          new MenuItem('Báo cáo thông tin tài sản', '/app/main/bao-cao/bao-cao-thong-tin-tai-san', '', 'Pages.BaoCaoThongTinTaiSan')
        ]),
      new MenuItem('Quản lý Danh mục', '', 'fas fa-th-large',
        this.getPermission('Pages.QuanLyNhaCungCap', 'Pages.QuanLyViTriDiaLy', 'Pages.QuanLyLoaiTaiSan', 'Pages.QuanLyPhongBan'),
        [
          new MenuItem('Quản lý Nhà cung cấp', '/app/main/nha-cung-cap', '', 'Pages.QuanLyNhaCungCap'),
          new MenuItem('Quản lý Vị trí địa lý', '/app/main/vi-tri-dia-ly', '', 'Pages.QuanLyViTriDiaLy'),
          new MenuItem('Quản lý Loại tài sản', '/app/main/loai-tai-san', '', 'Pages.QuanLyLoaiTaiSan'),
          new MenuItem('Quản lý Đơn vị', '/app/main/to-chuc', '', 'Pages.QuanLyPhongBan'),
        ]),

      new MenuItem('Quản lý Hệ thống', '', 'fas fa-users',
        this.getPermission('Pages.Users', 'Pages.Roles', 'Pages.QuanLyLichSuNguoiDung', 'Pages.QuanLyMailServer'),
        [
          new MenuItem('Quản lý Người dùng', '/app/users', '', 'Pages.Users'),
          new MenuItem('Quản lý Phân quyền', '/app/roles', '', 'Pages.Roles'),
          new MenuItem('Lịch sử Người dùng', '/app/auditLogs', '', 'Pages.QuanLyLichSuNguoiDung'),
          new MenuItem('Quản lý Mail Server', '/app/main/cau-hinh-mail-server', '', 'Pages.QuanLyMailServer'),
        ]),
    ];
  }

  getPermission(...permissions: string[]) {
    let checkPermission = false;
    permissions.forEach(element => {
      if (this.isGrantedAny(element)) {
        checkPermission = true;
      }
    });
    return checkPermission ? '' : 'abc';
  }

  patchMenuItems(items: MenuItem[], parentId?: number): void {
    items.forEach((item: MenuItem, index: number) => {
      item.id = parentId ? Number(parentId + '' + (index + 1)) : index + 1;
      if (parentId) {
        item.parentId = parentId;
      }
      if (parentId || item.children) {
        this.menuItemsMap[item.id] = item;
      }
      if (item.children) {
        this.patchMenuItems(item.children, item.id);
      }
    });
  }

  activateMenuItems(url: string): void {
    this.deactivateMenuItems(this.menuItems);
    this.activatedMenuItems = [];
    const foundedItems = this.findMenuItemsByUrl(url, this.menuItems);
    foundedItems.forEach((item) => {
      this.activateMenuItem(item);
    });
  }

  deactivateMenuItems(items: MenuItem[]): void {
    items.forEach((item: MenuItem) => {
      item.isActive = false;
      item.isCollapsed = true;
      if (item.children) {
        this.deactivateMenuItems(item.children);
      }
    });
  }

  findMenuItemsByUrl(
    url: string,
    items: MenuItem[],
    foundedItems: MenuItem[] = []
  ): MenuItem[] {
    items.forEach((item: MenuItem) => {
      if (item.route === url) {
        foundedItems.push(item);
      } else if (item.children) {
        this.findMenuItemsByUrl(url, item.children, foundedItems);
      }
    });
    return foundedItems;
  }

  activateMenuItem(item: MenuItem): void {
    item.isActive = true;
    if (item.children) {
      item.isCollapsed = false;
    }
    this.activatedMenuItems.push(item);
    if (item.parentId) {
      this.activateMenuItem(this.menuItemsMap[item.parentId]);
    }
  }

  isMenuItemVisible(item: MenuItem): boolean {
    if (!item.permissionName) {
      return true;
    }
    return this.permission.isGranted(item.permissionName);
  }
}
