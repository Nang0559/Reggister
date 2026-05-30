//// File: leaveCalendar.js
//// Đặt tại: FVN_REGISTER.Shared/wwwroot/js/leaveCalendar.js

window.leaveCalendar = (function () {

    let _calendar = null;
    let _dotNetRef = null;
    let _holidayDates = [];

    // ─── Helpers ───────────────────────────────────────────
    function isWeekend(date) {
        const day = date.getDay();
        return day === 0 || day === 6;
    }

    function isHoliday(dateStr) {
        return _holidayDates.includes(dateStr);
    }

    function toDateStr(date) {
        // Chuyển Date object → "yyyy-MM-dd" theo local time (tránh UTC offset lệch ngày)
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    }

    // Kiểm tra range có hợp lệ không (không phải toàn cuối tuần/lễ)
    function hasValidDayInRange(startDate, endDate) {
        const d = new Date(startDate);
        while (d < endDate) {
            const dateStr = toDateStr(d);
            if (!isWeekend(d) && !isHoliday(dateStr)) return true;
            d.setDate(d.getDate() + 1);
        }
        return false;
    }

    // ─── Init ──────────────────────────────────────────────
    function init(element, dotNetRef, events, holidayDates) {
        _dotNetRef = dotNetRef;
        _holidayDates = holidayDates || [];

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

            selectable: true,
            selectMirror: true,
            unselectAuto: true,

            selectAllow: function (selectInfo) {
                return hasValidDayInRange(selectInfo.start, selectInfo.end);
            },

            events: events,

            // ✅ Chỉ dùng select, BỎ dateClick để tránh gọi 2 lần
            select: function (info) {
                const startStr = toDateStr(info.start);

                const endExclusive = new Date(info.end);
                endExclusive.setDate(endExclusive.getDate() - 1);
                const endStr = toDateStr(endExclusive);

                if (_dotNetRef) {
                    _dotNetRef.invokeMethodAsync('OnRangeSelect', startStr, endStr)
                        .catch(err => console.error('[leaveCalendar] OnRangeSelect error:', err));
                }

                _calendar.unselect();
            },

            // ✅ BỎ dateClick — select đã xử lý cả click 1 ngày

            eventClick: function (info) {
                info.jsEvent.stopPropagation();

                const eventId = info.event.id;
                const status = info.event.extendedProps?.status ?? '';

                if (_dotNetRef) {
                    _dotNetRef.invokeMethodAsync('OnEventClick', String(eventId), String(status))
                        .catch(err => console.error('[leaveCalendar] OnEventClick error:', err));
                }
            },

            dayCellClassNames: function (arg) {
                const classes = [];
                if (isWeekend(arg.date)) classes.push('fc-weekend');
                if (isHoliday(toDateStr(arg.date))) classes.push('fc-holiday');
                return classes;
            },

            dayCellDidMount: function (arg) {
                if (isHoliday(toDateStr(arg.date))) {
                    arg.el.setAttribute('title', 'Ngày nghỉ công ty');
                }
            },

            eventContent: function (arg) {
                const status = arg.event.extendedProps?.status ?? '';
                const title = arg.event.title;

                const icons = {
                    'Approved': '✓',
                    'Rejected': '✗',
                    'Pending': '⏳',
                    'ApprovedLv1': '½',
                    'ApprovedLv2': '⅔',
                    'Holiday': '🏖',
                };
                const icon = icons[status] || '';

                return {
                    html: `<div class="fc-event-custom" title="${status}">
                           <span class="fc-event-icon">${icon}</span>
                           <span class="fc-event-label">${title}</span>
                       </div>`
                };
            }
        });

        _calendar.render();
    }

    // ─── Cập nhật events sau khi submit ────────────────────
    function updateEvents(newEvents) {
        if (!_calendar) return;
        _calendar.getEvents().forEach(e => e.remove());
        newEvents.forEach(e => _calendar.addEvent(e));
    }

    // ─── Cập nhật danh sách ngày lễ (đổi năm) ─────────────
    function updateHolidays(newHolidays) {
        _holidayDates = newHolidays || [];
        if (_calendar) _calendar.render();
    }

    // ─── Destroy ───────────────────────────────────────────
    function destroy() {
        if (_calendar) {
            _calendar.destroy();
            _calendar = null;
        }
        _dotNetRef = null;
        _holidayDates = [];
    }

    return { init, updateEvents, updateHolidays, destroy };

})();


