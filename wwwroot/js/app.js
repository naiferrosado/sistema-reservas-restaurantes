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
        card.addEventListener('click', function (e) {
            // Si el clic fue en un botón de acción (editar / eliminar), ignorar toggle
            if (e.target.closest('.table-action-btn') || e.target.closest('[data-modal-open]')) {
                return;
            }

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
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
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

            // Transferir datos para edición de platillo
            if (modalId === 'modal-menu-edit') {
                var mId = document.getElementById('edit-menu-id');
                var mName = document.getElementById('edit-menu-name');
                var mDesc = document.getElementById('edit-menu-desc');
                var mPrice = document.getElementById('edit-menu-price');
                var mCat = document.getElementById('edit-menu-category');
                var mImg = document.getElementById('edit-menu-image');
                var mTags = document.getElementById('edit-menu-tags');
                var mAvail = document.getElementById('edit-menu-available');

                if (mId) mId.value = this.dataset.menuId || '';
                if (mName) mName.value = this.dataset.menuName || '';
                if (mDesc) mDesc.value = this.dataset.menuDesc || '';
                if (mPrice) mPrice.value = this.dataset.menuPrice || '';
                if (mCat && this.dataset.menuCat) mCat.value = this.dataset.menuCat;
                if (mImg) mImg.value = this.dataset.menuImg || '';
                if (mTags) mTags.value = this.dataset.menuTags || '';
                if (mAvail) mAvail.checked = (this.dataset.menuAvailable === 'true');
            }

            // Transferir datos para eliminación de platillo
            if (modalId === 'modal-menu-delete') {
                var delMenuId = document.getElementById('delete-menu-id');
                var delMenuName = document.querySelector('#modal-menu-delete .modal-menu-name');
                if (delMenuId) delMenuId.value = this.dataset.menuId || '';
                if (delMenuName) delMenuName.textContent = '\"' + (this.dataset.menuName || '') + '\"';
            }

            // Transferir datos para edición de mesa
            if (modalId === 'modal-table-edit') {
                var tId = document.getElementById('edit-table-id');
                var tNum = document.getElementById('edit-table-number');
                var tCap = document.getElementById('edit-table-capacity');
                var tLoc = document.getElementById('edit-table-location');
                var tStat = document.getElementById('edit-table-status');

                if (tId) tId.value = this.dataset.tableId || '';
                if (tNum) tNum.value = this.dataset.tableNumber || '';
                if (tCap) tCap.value = this.dataset.tableCapacity || '';
                if (tLoc && this.dataset.tableLocation) tLoc.value = this.dataset.tableLocation;
                if (tStat && this.dataset.tableStatus) tStat.value = this.dataset.tableStatus;
            }

            // Transferir datos para eliminación de mesa
            if (modalId === 'modal-table-delete') {
                var delTableId = document.getElementById('delete-table-id');
                var delTableName = document.querySelector('#modal-table-delete .modal-table-name');
                if (delTableId) delTableId.value = this.dataset.tableId || '';
                if (delTableName) delTableName.textContent = 'Mesa ' + (this.dataset.tableNumber || '');
            }

            // Transferir datos para edición de restaurante
            if (modalId === 'modal-restaurant-edit') {
                var rId = document.getElementById('edit-restaurant-id');
                var rName = document.getElementById('edit-restaurant-name');
                var rDesc = document.getElementById('edit-restaurant-description');
                var rAddr = document.getElementById('edit-restaurant-address');
                var rPhone = document.getElementById('edit-restaurant-phone');
                var rEmail = document.getElementById('edit-restaurant-email');
                var rCuisine = document.getElementById('edit-restaurant-cuisinetype');
                var rOpen = document.getElementById('edit-restaurant-openingtime');
                var rClose = document.getElementById('edit-restaurant-closingtime');
                var rImg = document.getElementById('edit-restaurant-imageurl');
                var rRating = document.getElementById('edit-restaurant-rating');
                var rPrice = document.getElementById('edit-restaurant-pricerange');
                var rCap = document.getElementById('edit-restaurant-totalcapacity');

                if (rId) rId.value = this.dataset.restaurantId || '';
                if (rName) rName.value = this.dataset.restaurantName || '';
                if (rDesc) rDesc.value = this.dataset.restaurantDescription || '';
                if (rAddr) rAddr.value = this.dataset.restaurantAddress || '';
                if (rPhone) rPhone.value = this.dataset.restaurantPhone || '';
                if (rEmail) rEmail.value = this.dataset.restaurantEmail || '';
                if (rCuisine) rCuisine.value = this.dataset.restaurantCuisinetype || '';
                if (rOpen) rOpen.value = this.dataset.restaurantOpeningtime || '';
                if (rClose) rClose.value = this.dataset.restaurantClosingtime || '';
                if (rImg) rImg.value = this.dataset.restaurantImageurl || '';
                if (rRating) rRating.value = this.dataset.restaurantRating || '4.8';
                if (rPrice) rPrice.value = this.dataset.restaurantPricerange || '$$$';
                if (rCap) rCap.value = this.dataset.restaurantTotalcapacity || '50';
            }

            // Transferir datos para eliminación de restaurante
            if (modalId === 'modal-restaurant-delete') {
                var delRestId = document.getElementById('delete-restaurant-id');
                var delRestName = document.querySelector('#modal-restaurant-delete .modal-restaurant-name');
                if (delRestId) delRestId.value = this.dataset.restaurantId || '';
                if (delRestName) delRestName.textContent = '\"' + (this.dataset.restaurantName || '') + '\"';
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
