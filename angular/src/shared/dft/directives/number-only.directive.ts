/* tslint:disable */
import { Directive, ElementRef, AfterViewInit, EventEmitter, Output, Input } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
    selector: '[number]'
})
export class NumberDirective implements AfterViewInit {
    private _isInitialized = false;
    @Input() max: number;
    @Output() onValueChange: EventEmitter<number> = new EventEmitter();
    constructor(private _element: ElementRef, private control: NgControl) {
    }

    ngAfterViewInit() {
        if (this._isInitialized) {
            return;
        }

        $(this._element.nativeElement).inputmask('numeric', {
            rightAlign: false,
            digits: 0,
            groupSeparator: ',',
            radixPoint: '.',
            autoGroup: true,
            removeMaskOnSubmit: true,
            autoUnmask: true,
            oncomplete: () => {
                let value = Number(this._element.nativeElement.value);
                if (this.max && value > this.max) {
                    value = this.max;
                }
                this.control.control.setValue(value);
                this.onValueChange.emit(value);
            }, oncleared: () => {
                this.control.control.setValue(null);
                this.onValueChange.emit(null);
            }
        });

        this._isInitialized = true;

    }
}
