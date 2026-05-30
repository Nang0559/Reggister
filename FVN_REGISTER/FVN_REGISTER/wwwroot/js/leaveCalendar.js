// File: leaveCalendar.js
// Đặt tại: FVN_REGISTER.Shared/wwwroot/js/leaveCalendar.js
// Cần thêm vào _Layout.cshtml hoặc wwwroot/index.html:
//   <link href="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.11/index.global.min.css" rel="stylesheet">
//   <script src="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.11/index.global.min.js"></script>
//   <script src="_content/FVN_REGISTER.Shared/js/leaveCalendar.js"></script>

window.leaveCalendar = (function () {

    let _calendar = null;
    let _dotNetRef = null;

    function init(element, dotNetRef, events, holidayDates) {
        _dotNetRef = dotNetRef;

        if (_calendar) {
            _calendar.destroy();
            _calendar = null;
        }

        _calendar = new FullCalendar.Calendar(element, {
            initialView: 'dayGridMonth',
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

    function updateEvents(newEvents) {
        if (!_calendar) return;

        // Xóa hết events cũ
        _calendar.getEvents().forEach(e => e.remove());

        // Thêm events mới
        newEvents.forEach(e => _calendar.addEvent(e));
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
