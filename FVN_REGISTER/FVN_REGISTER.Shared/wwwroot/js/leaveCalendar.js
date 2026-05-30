// File: leaveCalendar.js
// Đặt tại: FVN_REGISTER.Shared/wwwroot/js/leaveCalendar.js
// Cần thêm vào _Layout.cshtml hoặc wwwroot/index.html:
//   <link href="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.11/index.global.min.css" rel="stylesheet">
//   <script src="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.11/index.global.min.js"></script>
//   <script src="_content/FVN_REGISTER.Shared/js/leaveCalendar.js"></script>

window.leaveCalendar = (function () {

    let _calendar = null;
    let _dotNetRef = null;

    // SỬA: Thêm tham số selectedYear vào cuối hàm init
    function init(element, dotNetRef, events, holidayDates, selectedYear) {
        _dotNetRef = dotNetRef;

        if (_calendar) {
            _calendar.destroy();
            _calendar = null;
        }

        // Nếu không truyền selectedYear hoặc truyền sai, tự động lấy năm hiện tại
        const targetYear = selectedYear || new Date().getFullYear();

        _calendar = new FullCalendar.Calendar(element, {
            initialView: 'dayGridMonth',

            // SỬA CHÍ MẠNG: Ép FullCalendar nhảy đúng về ngày đầu tiên của năm đang chọn
            initialDate: `${targetYear}-01-01`,

            locale: 'vi',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,listMonth'
            },
            buttonText: {
                today: 'Hôm nay',
                month: 'Tháng',
                list: 'Danh sách'
            },
            height: 'auto',
            selectable: false,
            events: events,

            // Click vào ô ngày trống → mở form đăng ký
            dateClick: function (info) {
                const dateStr = info.dateStr;

                // Kiểm tra cuối tuần
                const day = info.date.getDay();
                if (day === 0 || day === 6) {
                    return;
                }

                // Kiểm tra ngày nghỉ
                const isHoliday = holidayDates && holidayDates.includes(dateStr);
                if (isHoliday) {
                    return;
                }

                if (_dotNetRef) {
                    _dotNetRef.invokeMethodAsync('OnDateClick', dateStr);
                }
            },

            // Click vào event đã có → xem chi tiết
            eventClick: function (info) {
                const eventId = info.event.id;
                const status = info.event.extendedProps?.status ?? '';
                if (_dotNetRef) {
                    _dotNetRef.invokeMethodAsync('OnEventClick', String(eventId), String(status));
                }
            },

            // Style ngày cuối tuần
            dayCellClassNames: function (arg) {
                const day = arg.date.getDay();
                if (day === 0 || day === 6) return ['fc-weekend'];
                return [];
            },

            // Style custom cho event
            eventContent: function (arg) {
                const status = arg.event.extendedProps?.status;
                const title = arg.event.title;
                return {
                    html: `<div class="fc-event-custom" title="${status}">
                               <span class="fc-event-dot"></span>
                               <span class="fc-event-label">${title}</span>
                           </div>`
                };
            }
        });

        _calendar.render();
    }

    // Tối ưu lại hàm cập nhật sự kiện mượt mà hơn
    function updateEvents(newEvents) {
        if (!_calendar) return;

        // Xóa các nguồn sự kiện cũ một cách an toàn
        const sources = _calendar.getEventSources();
        sources.forEach(source => source.remove());

        // Thêm mảng sự kiện mới trực tiếp vào nguồn cấp của FullCalendar
        _calendar.addEventSource(newEvents);
    }

    function destroy() {
        if (_calendar) {
            _calendar.destroy();
            _calendar = null;
        }
        _dotNetRef = null;
    }

    return { init, updateEvents, destroy };

})();
