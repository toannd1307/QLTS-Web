// tslint:disable
import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  Injector
} from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'abp-modal-footer',
  templateUrl: './abp-modal-footer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AbpModalFooterComponent extends AppComponentBase {
  @Input() cancelLabel = 'Hủy';
  @Input() iconName = 'fa fa-save';
  @Input() cancelDisabled: boolean;
  @Input() cancelHidden = false;
  @Input() saveLabel = 'Lưu';
  @Input() saveDisabled: boolean;
  @Input() saveHidden = false;
  @Output() onCancelClick = new EventEmitter<number>();

  constructor(injector: Injector) {
    super(injector);
  }
}
