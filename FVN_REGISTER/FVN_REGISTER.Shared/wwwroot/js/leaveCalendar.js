//// File: leaveCalendar.js
//// Đặt tại: FVN_REGISTER.Shared/wwwroot/js/leaveCalendar.js

window.leaveCalendar = (function () {

    let _calendar = null;
    let _dotNetRef = null;
    let _holidayDates = [];
    let _selectGuard = false; // ✅ Tránh dateClick + select gọi 2 lần

    // ─── Helpers ───────────────────────────────────────────
    function isWeekend(date) {
        const day = date.getDay();
        return day === 0 || day === 6;
    }

    function isHoliday(dateStr) {
        return _holidayDates.includes(dateStr);
    }

    function toDateStr(date) {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    }

    function hasValidDayInRange(startDate, endDate) {
        const d = new Date(startDate);
        while (d < endDate) {
            const dateStr = toDateStr(d);
            if (!isWeekend(d) && !isHoliday(dateStr)) return true;
            d.setDate(d.getDate() + 1);
        }
        return false;
    }

    function isMobileDevice() {
        return window.innerWidth < 768
            || 'ontouchstart' in window
            || navigator.maxTouchPoints > 0;
    }

    function invokeSelect(startStr, endStr) {
        if (_selectGuard) return;
        _selectGuard = true;

        if (_dotNetRef) {
            _dotNetRef.invokeMethodAsync('OnRangeSelect', startStr, endStr)
                .catch(err => console.error('[leaveCalendar] OnRangeSelect error:', err));
        }

        // Reset guard sau 600ms (đủ để bỏ qua event trùng lặp)
        setTimeout(() => { _selectGuard = false; }, 600);
    }

    // ─── Init ──────────────────────────────────────────────
    function init(element, dotNetRef, events, holidayDates) {
        _dotNetRef = dotNetRef;
        _holidayDates = holidayDates || [];
        _selectGuard = false;

        if (_calendar) {
            _calendar.destroy();
            _calendar = null;
        }

        const mobile = isMobileDevice();

        _calendar = new FullCalendar.Calendar(element, {
            initialView: 'dayGridMonth', // ✅ Luôn dùng dayGridMonth (listMonth không hỗ trợ select)
            locale: 'vi',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: mobile ? 'dayGridMonth' : 'dayGridMonth,listMonth'
            },
            buttonText: {
                today: 'Hôm nay',
                month: 'Tháng',
                list: 'Danh sách'
            },
            height: 'auto',

            // ✅ MOBILE TOUCH CONFIG
            selectable: true,
            selectMirror: true,
            unselectAuto: true,
            longPressDelay: mobile ? 400 : 0,       // Giữ ngón tay 400ms để bắt đầu select
            selectLongPressDelay: mobile ? 400 : 0, // Riêng cho select gesture

            selectAllow: function (selectInfo) {
                return hasValidDayInRange(selectInfo.start, selectInfo.end);
            },

            events: events,

            // ✅ select: xử lý kéo chọn range (desktop) + long-press drag (mobile)
            select: function (info) {
                const startStr = toDateStr(info.start);

                const endExclusive = new Date(info.end);
                endExclusive.setDate(endExclusive.getDate() - 1);
                const endStr = toDateStr(endExclusive);

                invokeSelect(startStr, endStr);
                _calendar.unselect();
            },

            // ✅ dateClick: fallback cho mobile single-tap (tap nhanh không trigger select)
            dateClick: function (info) {
                invokeSelect(info.dateStr, info.dateStr);
            },

            eventClick: function (info) {
                info.jsEvent.stopPropagation();
                // ✅ Ngăn eventClick trigger dateClick/select bên dưới
                _selectGuard = true;
                setTimeout(() => { _selectGuard = false; }, 600);

                const detailId = info.event.id;
                const leaveId = info.event.extendedProps?.inforregister ?? '';
                const status = info.event.extendedProps?.APstatus
                    ?? info.event.extendedProps?.status ?? '';
                const startDate = info.event.startStr;

                if (_dotNetRef) {
                    _dotNetRef.invokeMethodAsync(
                        'OnEventClick',
                        String(detailId),
                        String(leaveId),
                        String(status),
                        String(startDate)
                    ).catch(err => console.error('[leaveCalendar] OnEventClick error:', err));
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

    // ─── Cập nhật events ───────────────────────────────────
    function updateEvents(newEvents) {
        if (!_calendar) return;
        _calendar.getEvents().forEach(e => e.remove());
        newEvents.forEach(e => _calendar.addEvent(e));
    }

    // ─── Cập nhật ngày lễ ──────────────────────────────────
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
        _selectGuard = false;
    }

    return { init, updateEvents, updateHolidays, destroy };

})();


