/* tslint:disable */
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'tienTe' })

export class TienTePipe implements PipeTransform {
    transform(value: any): string {
        if (value == null) {
            return 0 + '';
        }
        return !isNaN(value) ? value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',') : value.toString();
    }
}
