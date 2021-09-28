import { Directive, ElementRef, HostListener } from '@angular/core';

@Directive({
  selector: '[appPhoneNumber]'
})
export class PhoneNumberDirective {
  constructor(private _element: ElementRef) {
  }

  @HostListener('keypress', ['$event']) onKeyPress(event: KeyboardEvent) {
    const numberArr = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+'];
    if (numberArr.includes(event.key)) {
      if (event.key === '+' && this._element.nativeElement.selectionStart !== 0) {
        return this._element.nativeElement.value.length === 0;
      }
      return true;
    }
    return false;
  }
}
