/* tslint:disable */
import { Directive, ElementRef, AfterViewInit, Input, EventEmitter, Output } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
    selector: '[decimal]'
})
export class DecimalDirective implements AfterViewInit {

    @Input('decimal') decimal: any;
    @Input() max: number;
    @Output() onValueChange: EventEmitter<string> = new EventEmitter();
    private _isInitialized = false;

    constructor(private _element: ElementRef, private control: NgControl) {

    }

    ngAfterViewInit() {
        if (this._isInitialized) {
            return;
        }

        $(this._element.nativeElement).inputmask('numeric', {
            digits: this.decimal != '' ? this.decimal[0]['digits'] : 2,
            integerDigits: this.decimal != '' ? this.decimal[0]['integerDigits'] : 20,
            groupSeparator: this.decimal != '' ? this.decimal[0]['groupSeparator'] : ',',
            radixPoint: this.decimal != '' ? this.decimal[0]['radixPoint'] : '.',
            autoGroup: this.decimal != '' ? this.decimal[0]['autoGroup'] : true,
            rightAlign: this.decimal != '' ? this.decimal[0]['rightAlign'] : false,
            autoUnmask: this.decimal != '' ? this.decimal[0]['autoUnmask'] : true,
            prefix: this.decimal != '' ? this.decimal[0]['prefix'] : '',
            allowPlus: this.decimal != '' && this.decimal[0]['allowPlus'] != undefined ? this.decimal[0]['allowPlus'] : true,
            allowMinus: this.decimal != '' && this.decimal[0]['allowMinus'] != undefined ? this.decimal[0]['allowMinus'] : true,
            oncomplete: (value) => {
                let start = value.target.selectionStart;
                let end = value.target.selectionEnd;
                let dot = this.decimal != '' && this.decimal[0]['radixPoint'] != undefined ? this.decimal[0]['radixPoint'] : '.';
                if (this._element.nativeElement.value[this._element.nativeElement.value.length - 1] != dot) {
                    let controlValue = Number(this._element.nativeElement.value);
                    if (this.max && controlValue > this.max) {
                        this.control.control.setValue(this.max.toString());
                        value.target.selectionStart = this._element.nativeElement.value.length;
                        value.target.selectionEnd = this._element.nativeElement.value.length;
                        this.onValueChange.emit(this.max.toString());
                    } else {
                        this.control.control.setValue(this._element.nativeElement.value);
                        value.target.selectionStart = start;
                        value.target.selectionEnd = end;
                        this.onValueChange.emit(this._element.nativeElement.value);
                    }

                }
            }, oncleared: () => {
                this.control.control.setValue(null);
                this.onValueChange.emit(this._element.nativeElement.value);
            }
        });

        this._isInitialized = true;
    }

}
