window.workCalendar = (function () {
    let _calendar = null;
    let _dotNetRef = null;
    let _days = new Map();

    function dayKey(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return year + '-' + month + '-' + day;
    }

    function clearCustomContent(cell) {
        cell.querySelectorAll('.fcc-calendar-day-content, .fcc-calendar-issue-marker').forEach(x => x.remove());
    }

    function appendLine(container, text, className) {
        if (!text) return;

        const line = document.createElement('div');
        line.className = className || 'fcc-calendar-day-line';
        line.textContent = text;
        container.appendChild(line);
    }

    function renderCell(arg) {
        const key = dayKey(arg.date);
        const day = _days.get(key);

        clearCustomContent(arg.el);

        if (!day) return;

        const top = arg.el.querySelector('.fc-daygrid-day-top');
        const frame = arg.el.querySelector('.fc-daygrid-day-frame');

        if (!frame) return;

        const critical = (day.issues || []).some(x => Number(x.severity || 0) >= 3);
        const warning = (day.issues || []).some(x => Number(x.severity || 0) === 2);

        if ((day.issues || []).length > 0 && top) {
            const marker = document.createElement('button');
            marker.type = 'button';
            marker.className =
                'fcc-calendar-issue-marker ' +
                (critical ? 'critical' : warning ? 'warning' : 'info');

            marker.textContent = '?';
            marker.title = 'Có vấn đề cần xử lý';

            marker.addEventListener('click', function (event) {
                event.preventDefault();
                event.stopPropagation();

                const first = day.issues[0];

                if (_dotNetRef) {
                    _dotNetRef.invokeMethodAsync(
                        'OnCalendarIssueClick',
                        key,
                        first.code);
                }
            });

            top.appendChild(marker);
        }

        const body = document.createElement('div');
        body.className = 'fcc-calendar-day-content';

        if (day.holiday) {
            appendLine(
                body,
                'Nghỉ: ' + day.holiday,
                'fcc-calendar-day-line holiday');
        }

        if (day.shift) {
            appendLine(
                body,
                'Ca ' + day.shift,
                'fcc-calendar-day-line shift');
        }

        if (day.attendance) {
            const inText = day.attendance.checkIn || '--:--';
            const outText = day.attendance.checkOut || '--:--';

            appendLine(
                body,
                inText + ' → ' + outText,
                'fcc-calendar-day-line attendance');

            if (day.attendance.actualHours != null
                && day.attendance.requiredHours != null) {
                appendLine(
                    body,
                    Number(day.attendance.actualHours).toFixed(2).replace(/\.00$/, '') +
                        'h / ' +
                        Number(day.attendance.requiredHours).toFixed(2).replace(/\.00$/, '') +
                        'h',
                    'fcc-calendar-day-line hours');
            } else if (day.attendance.display) {
                appendLine(
                    body,
                    String(day.attendance.display),
                    'fcc-calendar-day-line hours');
            }
        }

        (day.registrations || []).forEach(function (registration) {
            const prefix = registration.isHalfDay ? '½ ' : '';

            appendLine(
                body,
                prefix + registration.moduleCode + ' · ' + registration.title,
                'fcc-calendar-day-line registration ' +
                    String(registration.moduleCode).toLowerCase());
        });

        if (day.canRegister) {
            appendLine(
                body,
                '+ Đăng ký',
                'fcc-calendar-day-line registration-open');
        }

        frame.appendChild(body);
    }

    function renderAllCells() {
        if (!_calendar) return;

        _calendar.el
            .querySelectorAll('.fc-daygrid-day')
            .forEach(function (cell) {
                const date = cell.getAttribute('data-date');

                if (!date) return;

                renderCell({
                    date: new Date(date + 'T00:00:00'),
                    el: cell
                });
            });
    }

    function setDays(days) {
        _days = new Map();

        (days || []).forEach(function (day) {
            _days.set(day.date, day);
        });
    }

    function init(element, dotNetRef, days, initialDate) {
        _dotNetRef = dotNetRef;
        setDays(days);

        if (_calendar) {
            _calendar.destroy();
        }

        _calendar = new FullCalendar.Calendar(element, {
            initialView: 'dayGridMonth',
            initialDate: initialDate,
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
            events: [],

            dateClick: function (info) {
                if (_dotNetRef) {
                    _dotNetRef.invokeMethodAsync(
                        'OnCalendarDateClick',
                        info.dateStr);
                }
            },

            datesSet: function (info) {
                if (_dotNetRef) {
                    _dotNetRef.invokeMethodAsync(
                        'OnCalendarRangeChanged',
                        info.startStr,
                        info.endStr);
                }
            },

            dayCellClassNames: function (arg) {
                const key = dayKey(arg.date);
                const day = _days.get(key);
                const classes = [];
                const weekDay = arg.date.getDay();

                if (weekDay === 0 || weekDay === 6) {
                    classes.push('fc-weekend');
                }

                if (day?.holiday) {
                    classes.push('fcc-company-holiday');
                }

                if (day?.canRegister) {
                    classes.push('fcc-registration-open');
                }

                if ((day?.issues || []).some(x => Number(x.severity || 0) >= 3)) {
                    classes.push('fcc-day-critical');
                } else if ((day?.issues || []).some(x => Number(x.severity || 0) === 2)) {
                    classes.push('fcc-day-warning');
                }

                return classes;
            },

            dayCellDidMount: renderCell
        });

        _calendar.render();
    }

    function updateDays(days) {
        if (!_calendar) return;

        setDays(days);
        requestAnimationFrame(renderAllCells);
    }

    function destroy() {
        if (_calendar) {
            _calendar.destroy();
            _calendar = null;
        }

        _dotNetRef = null;
        _days = new Map();
    }

    return {
        init: init,
        updateDays: updateDays,
        destroy: destroy
    };
})();