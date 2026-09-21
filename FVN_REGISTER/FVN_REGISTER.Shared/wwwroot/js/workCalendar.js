
window.workCalendar = (function () {
    let _calendar = null;
    let _dotNetRef = null;
    let _registrationDates = new Set();

    function init(element, dotNetRef, events, initialDate, registrationDates) {
        _dotNetRef = dotNetRef;
        _registrationDates = new Set(registrationDates || []);
        if (_calendar) _calendar.destroy();

        _calendar = new FullCalendar.Calendar(element, {
            initialView: 'dayGridMonth',
            initialDate: initialDate,
            locale: 'vi',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,listMonth'
            },
            buttonText: { today: 'Hôm nay', month: 'Tháng', list: 'Danh sách' },
            height: 'auto',
            selectable: false,
            events: events || [],

            dateClick: function (info) {
                if (_dotNetRef)
                    _dotNetRef.invokeMethodAsync('OnCalendarDateClick', info.dateStr);
            },

            eventClick: function (info) {
                if (_dotNetRef)
                    _dotNetRef.invokeMethodAsync('OnCalendarEventClick', String(info.event.id));
            },

            dayCellClassNames: function (arg) {
                const d = arg.date.getDay();
                const classes = (d === 0 || d === 6) ? ['fc-weekend'] : [];
                const key = arg.date.toISOString().slice(0, 10);
                if (_registrationDates.has(key))
                    classes.push('fc-registration-open');
                return classes;
            },
            
            dayCellDidMount: function (arg) {
                const key = arg.date.toISOString().slice(0, 10);
                if (!_registrationDates.has(key)) return;

                arg.el.title = 'Có thể đăng ký từ lịch';
                const marker = document.createElement('span');
                marker.textContent = '+ Đăng ký';
                marker.style.display = 'block';
                marker.style.marginTop = '2px';
                marker.style.fontSize = '10px';
                marker.style.fontWeight = '700';
                marker.style.cursor = 'pointer';
                marker.style.opacity = '0.75';
                arg.el.querySelector('.fc-daygrid-day-top')?.appendChild(marker);
            },

            eventContent: function (arg) {
                const module = arg.event.extendedProps?.moduleCode || '';
                const status = arg.event.extendedProps?.status || '';
                const severity = Number(arg.event.extendedProps?.severity || 0);
                const requiresAction = !!arg.event.extendedProps?.requiresAction;
                const title = arg.event.title || '';
                const wrapper = document.createElement('div');
                wrapper.className = 'fc-event-custom';
                wrapper.title = module + ' · ' + status;
                if (severity >= 2) {
                    wrapper.style.fontWeight = '700';
                    wrapper.style.borderLeft = severity >= 3 ? '4px solid var(--mud-palette-error)' : '4px solid var(--mud-palette-warning)';
                    wrapper.style.paddingLeft = '4px';
                    wrapper.style.color = severity >= 3 ? 'var(--mud-palette-error)' : 'var(--mud-palette-warning)';
                }
                if (requiresAction)
                    wrapper.style.cursor = 'pointer';

                const label = document.createElement('span');
                label.className = 'fc-event-label';
                label.textContent = title;
                wrapper.appendChild(label);
                return { domNodes: [wrapper] };
            }
        });

        _calendar.render();
    }

    function updateEvents(newEvents, registrationDates) {
        if (!_calendar) return;
        _registrationDates = new Set(registrationDates || []);
        _calendar.removeAllEvents();
        (newEvents || []).forEach(e => _calendar.addEvent(e));
    }

    function destroy(element) {
        if (_calendar) {
            _calendar.destroy();
            _calendar = null;
        }
        _dotNetRef = null;
        _registrationDates = new Set();
    }

    return { init, updateEvents, destroy };
})();
