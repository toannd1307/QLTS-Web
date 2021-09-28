import { AppComponentBase } from '../../app-component-base';
import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Injector, OnInit, Output, Input } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
    selector: 'app-import-excel-modal',
    templateUrl: 'import-excel-dialog.component.html'
})

export class ImportExcelDialogComponent extends AppComponentBase {
    uploading = false;
    saveDisabled = false;
    downLoading = false;
    maxFile = 1;
    files: File[] = [];
    returnMessage = '';
    excelAcceptTypes = '';
    @Output() onDownload = new EventEmitter();
    @Output() onSave = new EventEmitter<File[]>();
    @Output() onClose = new EventEmitter();
    constructor(
        public bsModalRef: BsModalRef,
        injector: Injector,
    ) {
        super(injector);
    }

    onSelect(event) {
        if (this.files.length < this.maxFile) {
            this.files.push(...event.addedFiles);
            this.saveDisabled = false;
        } else {
            this.showErrorMessage(`Chỉ được phép chọn tối đa ${this.maxFile} file.`);
        }
    }

    onRemove(event) {
        this.files.splice(this.files.indexOf(event), 1);
    }

    download(): void {
        this.downLoading = true;
        this.onDownload.emit();
    }

    upload() {
        this.uploading = true;
        this.onSave.emit(this.files);
    }

    uploadDone() {
        setTimeout(() => {
            this.saveDisabled = false;
            this.saveDisabled = true;
        });
    }

    close() {
        this.onClose.emit();
        this.bsModalRef.hide();
    }
}
