import { Pipe, PipeTransform } from '@angular/core';


@Pipe({
  name: 'statusstr', pure: true
})
export class StatusStrPipe implements PipeTransform {

  transform(value: any): any {
    switch (value) {
      case 'Đã kết thúc':
        return 2;
      case 'Đang kiểm kê':
        return 1;
      case 'Chưa bắt đầu':
        return 0;
      default:
        break;
    }
  }

}


