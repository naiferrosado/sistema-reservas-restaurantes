/* SistemaReservas — app.js
   Interactividad MVC: Plano de mesas, Modales, Toasts, Dashboard AJAX
   Sin dependencias externas (Vanilla JS puro)
*/

(function () {
    'use strict';

    // =====================================================================
    // AUTO-CIERRE DE TOASTS
    // =====================================================================
    document.querySelectorAll('.toast').forEach(function (toast) {
        setTimeout(function () {
            toast.style.animation = 'slideInRight 0.3s ease reverse';
            setTimeout(function () { toast.remove(); }, 300);
        }, 5000);
    });

    // =====================================================================
    // PLANO DE MESAS — Toggle de estado en tiempo real
    // =====================================================================
    document.querySelectorAll('.table-card[data-table-id]').forEach(function (card) {
        card.addEventListener('click', function () {
            var tableId = this.dataset.tableId;
            var cardEl = this;

            // Visual feedback inmediato
            cardEl.style.opacity = '0.6';
            cardEl.style.pointerEvents = 'none';

            fetch('/api/tables/' + tableId + '/toggle', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            })
            .then(function (res) {
                if (!res.ok) throw new Error('Error al cambiar estado');
                return res.json();
            })
            .then(function (data) {
                updateTableCard(cardEl, data.newStatus);
                refreshKpiCounters();
                showToast('Mesa ' + data.tableNumber + ': ' + data.newStatus, 'success');
            })
            .catch(function (err) {
                console.error(err);
                showToast('No se pudo actualizar el estado de la mesa.', 'error');
            })
            .finally(function () {
                cardEl.style.opacity = '';
                cardEl.style.pointerEvents = '';
            });
        });
    });

    function updateTableCard(card, newStatus) {
        // Eliminar clases de estado anteriores
        card.classList.remove('table-card--available', 'table-card--reserved', 'table-card--occupied');

        var statusClass = {
            'Disponible': 'table-card--available',
            'Reservada':  'table-card--reserved',
            'Ocupada':    'table-card--occupied'
        }[newStatus] || 'table-card--available';

        card.classList.add(statusClass);

        var statusEl = card.querySelector('.table-card-status span:last-child');
        if (statusEl) statusEl.textContent = newStatus;

        card.dataset.status = newStatus;
    }

    // =====================================================================
    // ACTUALIZAR KPIs TRAS CAMBIO DE MESA
    // =====================================================================
    function refreshKpiCounters() {
        var params = new URLSearchParams(window.location.search);
        var restaurantId = params.get('restaurantId') || '1';

        fetch('/api/dashboard/stats?restaurantId=' + restaurantId)
        .then(function (res) { return res.json(); })
        .then(function (data) {
            setKpiValue('kpi-reservations', data.totalReservationsToday);
            setKpiValue('kpi-guests', data.expectedGuestsToday);
            setKpiValue('kpi-occupancy', data.occupancyRate + '%');
            setKpiValue('kpi-pending', data.pendingCount);
        })
        .catch(function () {});
    }

    function setKpiValue(id, value) {
        var el = document.getElementById(id);
        if (el) {
            el.style.transition = 'none';
            el.style.transform = 'scale(1.1)';
            el.textContent = value;
            setTimeout(function () {
                el.style.transform = '';
                el.style.transition = 'transform 0.3s ease';
            }, 150);
        }
    }

    // =====================================================================
    // MODAL GENÉRICO
    // =====================================================================
    function openModal(modalId) {
        var modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.add('is-open');
            document.body.style.overflow = 'hidden';
        }
    }

    function closeModal(modalId) {
        var modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.remove('is-open');
            document.body.style.overflow = '';
        }
    }

    // Cerrar modal al hacer clic en overlay
    document.querySelectorAll('.modal-overlay').forEach(function (overlay) {
        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) {
                overlay.classList.remove('is-open');
                document.body.style.overflow = '';
            }
        });
    });

    // Botones de cierre de modal
    document.querySelectorAll('[data-modal-close]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var modalId = this.dataset.modalClose;
            closeModal(modalId);
        });
    });

    // Botones que abren modales
    document.querySelectorAll('[data-modal-open]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var modalId = this.dataset.modalOpen;
            // Transferir datos al modal (rechazo, previsualización)
            var reservationId = this.dataset.reservationId;
            var reservationCode = this.dataset.reservationCode;
            var emailBody = this.dataset.emailBody;

            if (reservationId) {
                var hiddenInput = document.querySelector('#' + modalId + ' [name="id"]');
                if (hiddenInput) hiddenInput.value = reservationId;
            }
            if (reservationCode) {
                var codeEl = document.querySelector('#' + modalId + ' .modal-reservation-code');
                if (codeEl) codeEl.textContent = reservationCode;
            }
            if (emailBody) {
                var previewEl = document.getElementById('email-preview-frame');
                if (previewEl) {
                    try {
                        previewEl.srcdoc = decodeURIComponent(emailBody);
                    } catch (e) {
                        previewEl.srcdoc = emailBody;
                    }
                }
            }

            openModal(modalId);
        });
    });

    // Escape para cerrar modal
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            document.querySelectorAll('.modal-overlay.is-open').forEach(function (m) {
                m.classList.remove('is-open');
                document.body.style.overflow = '';
            });
        }
    });

    // =====================================================================
    // TOAST DINÁMICO
    // =====================================================================
    function showToast(message, type) {
        type = type || 'success';

        var existing = document.getElementById('dynamic-toast');
        if (existing) existing.remove();

        var iconSuccess = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>';
        var iconError = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="15" x2="9" y1="9" y2="15"/><line x1="9" x2="15" y1="9" y2="15"/></svg>';

        var toast = document.createElement('div');
        toast.id = 'dynamic-toast';
        toast.className = 'toast toast--' + type;
        toast.innerHTML =
            '<div class="toast-icon">' + (type === 'success' ? iconSuccess : iconError) + '</div>' +
            '<span class="toast-message">' + escapeHtml(message) + '</span>' +
            '<button class="toast-close" onclick="this.parentElement.remove()">' +
            '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>' +
            '</button>';

        document.body.appendChild(toast);

        setTimeout(function () {
            if (toast.parentElement) {
                toast.style.animation = 'slideInRight 0.3s ease reverse';
                setTimeout(function () { toast.remove(); }, 300);
            }
        }, 4500);
    }

    // =====================================================================
    // FILTROS DE CATEGORÍA DEL MENÚ
    // =====================================================================
    document.querySelectorAll('[data-category-filter]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var category = this.dataset.categoryFilter;
            document.querySelectorAll('[data-category-filter]').forEach(function (b) {
                b.classList.remove('active');
            });
            this.classList.add('active');

            document.querySelectorAll('[data-menu-category]').forEach(function (section) {
                if (category === 'all' || section.dataset.menuCategory === category) {
                    section.style.display = '';
                } else {
                    section.style.display = 'none';
                }
            });
        });
    });

    // =====================================================================
    // HELPERS
    // =====================================================================
    function getAntiForgeryToken() {
        var token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    function escapeHtml(text) {
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(text));
        return div.innerHTML;
    }

    // Exponer funciones para uso desde HTML inline
    window.SistemaReservas = {
        openModal: openModal,
        closeModal: closeModal,
        showToast: showToast
    };

})();
