window.workCalendar = (function () {
    let _calendar = null;
    let _dotNetRef = null;

    function init(element, dotNetRef, events, initialDate) {
        _dotNetRef = dotNetRef;
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

            datesSet: function (info) {
                if (_dotNetRef)
                    _dotNetRef.invokeMethodAsync(
                        'OnCalendarRangeChanged',
                        info.startStr,
                        info.endStr);
            },

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
                return (d === 0 || d === 6) ? ['fc-weekend'] : [];
            },

            eventContent: function (arg) {
                const module = arg.event.extendedProps?.moduleCode || '';
                const status = arg.event.extendedProps?.status || '';
                const title = arg.event.title || '';
                const wrapper = document.createElement('div');
                wrapper.className = 'fc-event-custom';
                wrapper.title = module + ' · ' + status;
                const label = document.createElement('span');
                label.className = 'fc-event-label';
                label.textContent = title;
                wrapper.appendChild(label);
                return { domNodes: [wrapper] };
            }
        });

        _calendar.render();
    }

    function updateEvents(newEvents) {
        if (!_calendar) return;
        _calendar.removeAllEvents();
        (newEvents || []).forEach(e => _calendar.addEvent(e));
    }

    function destroy(element) {
        if (_calendar) {
            _calendar.destroy();
            _calendar = null;
        }
        _dotNetRef = null;
    }

    return { init, updateEvents, destroy };
})();