
 // tslint:disable
export class DashBoardConst {
    static backGroundChiTieu = '#ff7f0e';
    static backGroundDat = '#1f77b4';
    static backGroundDatCoGiaiTrinh = '#008080';
    static backGroundMTCLMang = '#009999';
    static backGroundKhongDat = '#ff0000';
    static backGroundTeal = '#08BC77';

    // MTCL Mang
    static backGroundChiTieuMTCL = '#61b15a';
    static backGroundDatMTCL = '#bedcfa';
    static backGroundDatCoGiaiTrinhMTCL = '#fff76a';
    static backGroundKhongDatMTCL = '#ff8e71';
    static backGroundTealMTCL1 = '#d1c145';
    static backGroundTealMTCL2 = '#d0e8f2';
    static backGroundTealMTCL3 = '#fca3cc';
    static backGroundTealMTCL4 = '#5aa469';
    static backGroundTealMTCL5 = '#79a3b1';
    static backGroundTealMTCL6 = '#898b8a';
    static backGroundTealMTCL7 = '#f9813a';
    static backGroundTealMTCL8 = '#32afa9';
    static backGroundTealMTCL9 = '#7189bf';
    static backGroundTealMTCL10 = '#7ed3b2';

    static nhanCong = '#01fa16';
    static csht = '#0052bd';
    static kenh = '#df9a06';
    static dien = '#c22bb5';
    static vhkt = '#e64100';
    static khauHaoChuyenTiep = '#10b491';
    static quanLy = '#ff7f0e';

    static animationBar = {
        duration: 500,
        onComplete() {
            const chartInstance = this.chart;
            const ctx = chartInstance.ctx;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'bottom';
            this.data.datasets.forEach((dataset, i) => {
                for (let j = 0; j < dataset.data.length; j++) {
                    const model = dataset._meta[Object.keys(dataset._meta)[0]].data[j]._model;
                    const scale_max = dataset._meta[Object.keys(dataset._meta)[0]].data[j]._yScale.maxHeight;
                    ctx.fillStyle = '#444';
                    let y_pos = model.y - 5;
                    // Make sure data value does not get overflown and hidden
                    // when the bar's value is too close to max value of scale
                    // Note: The y value is reverse, it counts from top down
                    if (((scale_max - model.y) / scale_max >= 0.93)) {
                        y_pos = model.y + 20;
                    }
                    ctx.fillText(dataset.data[j], model.x, y_pos);
                }
            });
        },
    };

    static animationPie = {
        duration: 500,
        easing: 'easeOutQuart',
        onComplete() {
            const ctx = this.chart.ctx;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'bottom';
            this.data.datasets.forEach((dataset) => {
                for (let i = 0; i < dataset.data.length; i++) {
                    const model = dataset._meta[Object.keys(dataset._meta)[0]].data[i]._model;
                    const mid_radius = model.innerRadius + (model.outerRadius - model.innerRadius) / 2;
                    const start_angle = model.startAngle;
                    const end_angle = model.endAngle;
                    const mid_angle = start_angle + (end_angle - start_angle) / 2;

                    const x = mid_radius * Math.cos(mid_angle);
                    const y = mid_radius * Math.sin(mid_angle);

                    ctx.fillStyle = '#fff';
                    if (i === 3) { // Darker text color for lighter background
                        ctx.fillStyle = '#444';
                    }

                    const val = dataset.data[i];
                    if (val !== 0) {
                        ctx.fillText(dataset.data[i] + '%', model.x + x, model.y + y);
                        // Display percent in another line, line break doesn't work for fillText
                        // ctx.fillText(percent, model.x + x, model.y + y + 15);
                    }
                }
            });
        },
    };
}
