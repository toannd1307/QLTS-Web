import { Pipe, PipeTransform } from '@angular/core';


@Pipe({
  name: 'statusnum', pure: true
})
export class StatusNumPipe implements PipeTransform {

  transform(value: any): any {
    switch (value) {
      case 0:
        return 'Chưa bắt đầu';
      case 1:
        return 'Đang kiểm kê';
      case 2:
        return 'Đã kết thúc';
      default:
        break;
    }
  }

}


