/**
 * SupportWeb - Sistema de Reclamos y Soporte
 * JavaScript principal para funcionalidades globales
 */

// Configuración global
window.SupportWeb = {
    config: {
        apiBaseUrl: '/api',
        toastDuration: 5000,
        chartColors: {
            primary: '#2563eb',
            success: '#059669',
            warning: '#d97706',
            danger: '#dc2626',
            info: '#0891b2',
            secondary: '#6b7280'
        }
    },

    // Estado global de la aplicación
    state: {
        notifications: [],
        user: null,
        unreadCount: 0
    }
};

/**
 * Utilidades generales
 */
const Utils = {
    // Formatear fechas
    formatDate: (dateString, includeTime = true) => {
        const date = new Date(dateString);
        const options = {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        };

        if (includeTime) {
            options.hour = '2-digit';
            options.minute = '2-digit';
        }

        return date.toLocaleDateString('es-ES', options);
    },

    // Formatear números
    formatNumber: (number) => {
        return new Intl.NumberFormat('es-ES').format(number);
    },

    // Truncar texto
    truncateText: (text, maxLength = 100) => {
        if (text.length <= maxLength) return text;
        return text.substring(0, maxLength).trim() + '...';
    },

    // Debounce función
    debounce: (func, wait) => {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    // Validar email
    isValidEmail: (email) => {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    },

    // Escapar HTML
    escapeHtml: (text) => {
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, (m) => map[m]);
    }
};

/**
 * Sistema de notificaciones toast
 */
const Toast = {
    container: null,

    init: () => {
        // Crear contenedor si no existe
        if (!Toast.container) {
            Toast.container = document.createElement('div');
            Toast.container.className = 'toast-container position-fixed top-0 end-0 p-3';
            Toast.container.style.zIndex = '9999';
            document.body.appendChild(Toast.container);
        }
    },

    show: (message, type = 'info', duration = window.SupportWeb.config.toastDuration) => {
        Toast.init();

        const toastId = 'toast-' + Date.now();
        const iconMap = {
            success: 'bi-check-circle-fill',
            error: 'bi-exclamation-triangle-fill',
            warning: 'bi-exclamation-triangle-fill',
            info: 'bi-info-circle-fill'
        };

        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-bg-${type === 'error' ? 'danger' : type} border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body d-flex align-items-center">
                        <i class="bi ${iconMap[type]} me-2"></i>
                        ${Utils.escapeHtml(message)}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;

        Toast.container.insertAdjacentHTML('beforeend', toastHtml);

        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { delay: duration });

        // Remover del DOM después de ocultarse
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });

        toast.show();
        return toast;
    },

    success: (message) => Toast.show(message, 'success'),
    error: (message) => Toast.show(message, 'error'),
    warning: (message) => Toast.show(message, 'warning'),
    info: (message) => Toast.show(message, 'info')
};

/**
 * Cliente HTTP para llamadas a la API
 */
const ApiClient = {
    request: async (url, options = {}) => {
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        };

        const config = { ...defaultOptions, ...options };

        try {
            const response = await fetch(url, config);

            // Manejar redirección a login
            if (response.status === 401) {
                Toast.warning('Sesión expirada. Redirigiendo al login...');
                setTimeout(() => {
                    window.location.href = '/Auth/Login';
                }, 2000);
                return null;
            }

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }

            return await response.text();
        } catch (error) {
            console.error('API request failed:', error);
            Toast.error('Error de conexión con el servidor');
            throw error;
        }
    },

    get: (url) => ApiClient.request(url),
    post: (url, data) => ApiClient.request(url, {
        method: 'POST',
        body: JSON.stringify(data)
    }),
    put: (url, data) => ApiClient.request(url, {
        method: 'PUT',
        body: JSON.stringify(data)
    }),
    delete: (url) => ApiClient.request(url, { method: 'DELETE' })
};

/**
 * Formularios con AJAX
 */
const FormHandler = {
    // Envío de formularios con AJAX
    submitForm: async (form, options = {}) => {
        const formData = new FormData(form);
        const action = form.action || window.location.href;
        const method = form.method || 'POST';

        // Mostrar loading
        const submitButton = form.querySelector('button[type="submit"]');
        const originalText = submitButton?.textContent;
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Procesando...';
        }

        try {
            const response = await fetch(action, {
                method: method.toUpperCase(),
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();

            if (result.success) {
                Toast.success(result.message || 'Operación completada exitosamente');

                // Callback de éxito
                if (options.onSuccess) {
                    options.onSuccess(result);
                }

                // Redirección si se especifica
                if (result.redirectUrl) {
                    setTimeout(() => {
                        window.location.href = result.redirectUrl;
                    }, 1500);
                }
            } else {
                Toast.error(result.message || 'Ocurrió un error al procesar la solicitud');

                // Mostrar errores de validación
                if (result.errors) {
                    FormHandler.showValidationErrors(form, result.errors);
                }
            }

            return result;
        } catch (error) {
            console.error('Form submission failed:', error);
            Toast.error('Error de conexión con el servidor');
            throw error;
        } finally {
            // Restaurar botón
            if (submitButton && originalText) {
                submitButton.disabled = false;
                submitButton.textContent = originalText;
            }
        }
    },

    // Mostrar errores de validación
    showValidationErrors: (form, errors) => {
        // Limpiar errores anteriores
        form.querySelectorAll('.invalid-feedback').forEach(el => el.remove());
        form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));

        // Mostrar nuevos errores
        Object.keys(errors).forEach(fieldName => {
            const field = form.querySelector(`[name="${fieldName}"]`);
            if (field) {
                field.classList.add('is-invalid');

                const errorDiv = document.createElement('div');
                errorDiv.className = 'invalid-feedback';
                errorDiv.textContent = errors[fieldName][0]; // Primer error

                field.parentNode.appendChild(errorDiv);
            }
        });
    },

    // Validación en tiempo real
    enableRealTimeValidation: (form) => {
        const fields = form.querySelectorAll('input, select, textarea');

        fields.forEach(field => {
            field.addEventListener('blur', () => {
                FormHandler.validateField(field);
            });

            field.addEventListener('input', Utils.debounce(() => {
                FormHandler.validateField(field);
            }, 500));
        });
    },

    // Validar campo individual
    validateField: (field) => {
        const value = field.value.trim();
        let isValid = true;
        let errorMessage = '';

        // Validaciones básicas
        if (field.hasAttribute('required') && !value) {
            isValid = false;
            errorMessage = 'Este campo es requerido';
        } else if (field.type === 'email' && value && !Utils.isValidEmail(value)) {
            isValid = false;
            errorMessage = 'Ingrese un email válido';
        } else if (field.hasAttribute('minlength') && value.length < parseInt(field.getAttribute('minlength'))) {
            isValid = false;
            errorMessage = `Mínimo ${field.getAttribute('minlength')} caracteres`;
        }

        // Actualizar UI
        field.classList.toggle('is-invalid', !isValid);
        field.classList.toggle('is-valid', isValid && value);

        // Mostrar/ocultar mensaje de error
        let errorDiv = field.parentNode.querySelector('.invalid-feedback');
        if (!isValid && errorMessage) {
            if (!errorDiv) {
                errorDiv = document.createElement('div');
                errorDiv.className = 'invalid-feedback';
                field.parentNode.appendChild(errorDiv);
            }
            errorDiv.textContent = errorMessage;
        } else if (errorDiv) {
            errorDiv.remove();
        }

        return isValid;
    }
};

/**
 * Gestión de modales
 */
const ModalManager = {
    // Abrir modal con contenido AJAX
    openAjaxModal: async (url, modalId = 'ajaxModal') => {
        try {
            // Crear modal si no existe
            let modal = document.getElementById(modalId);
            if (!modal) {
                modal = ModalManager.createModal(modalId);
            }

            // Mostrar loading
            const modalBody = modal.querySelector('.modal-body');
            modalBody.innerHTML = `
                <div class="text-center py-4">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <p class="mt-2 text-muted">Cargando contenido...</p>
                </div>
            `;

            // Mostrar modal
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();

            // Cargar contenido
            const content = await fetch(url, {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            }).then(response => response.text());

            modalBody.innerHTML = content;

            // Inicializar formularios en el modal
            const form = modalBody.querySelector('form');
            if (form) {
                FormHandler.enableRealTimeValidation(form);

                form.addEventListener('submit', async (e) => {
                    e.preventDefault();
                    const result = await FormHandler.submitForm(form);
                    if (result && result.success) {
                        bsModal.hide();
                        // Recargar página o tabla si es necesario
                        if (window.location.pathname.includes('/Reclamos')) {
                            window.location.reload();
                        }
                    }
                });
            }

            return bsModal;
        } catch (error) {
            console.error('Error loading modal content:', error);
            Toast.error('Error al cargar el contenido');
        }
    },

    // Crear modal dinámicamente
    createModal: (modalId) => {
        const modalHtml = `
            <div class="modal fade" id="${modalId}" tabindex="-1">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Modal</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body"></div>
                    </div>
                </div>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', modalHtml);
        return document.getElementById(modalId);
    },

    // Confirmar acción
    confirm: (message, title = 'Confirmar acción') => {
        return new Promise((resolve) => {
            const modalId = 'confirmModal-' + Date.now();
            const modalHtml = `
                <div class="modal fade" id="${modalId}" tabindex="-1">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">${Utils.escapeHtml(title)}</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                            </div>
                            <div class="modal-body">
                                <p>${Utils.escapeHtml(message)}</p>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                                <button type="button" class="btn btn-danger" id="confirmBtn">Confirmar</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            document.body.insertAdjacentHTML('beforeend', modalHtml);
            const modal = document.getElementById(modalId);
            const bsModal = new bootstrap.Modal(modal);

            // Event listeners
            modal.querySelector('#confirmBtn').addEventListener('click', () => {
                bsModal.hide();
                resolve(true);
            });

            modal.addEventListener('hidden.bs.modal', () => {
                modal.remove();
                resolve(false);
            });

            bsModal.show();
        });
    }
};

/**
 * Inicialización cuando el DOM está listo
 */
document.addEventListener('DOMContentLoaded', () => {
    console.log('SupportWeb initialized');

    // Inicializar tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

    // Inicializar popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl));

    // Manejar formularios con clase 'ajax-form'
    document.querySelectorAll('form.ajax-form').forEach(form => {
        FormHandler.enableRealTimeValidation(form);

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            await FormHandler.submitForm(form);
        });
    });

    // Manejar enlaces de modal AJAX
    document.addEventListener('click', (e) => {
        if (e.target.hasAttribute('data-ajax-modal')) {
            e.preventDefault();
            const url = e.target.getAttribute('href') || e.target.getAttribute('data-url');
            ModalManager.openAjaxModal(url);
        }

        // Manejar confirmaciones
        if (e.target.hasAttribute('data-confirm')) {
            e.preventDefault();
            const message = e.target.getAttribute('data-confirm');

            ModalManager.confirm(message).then(confirmed => {
                if (confirmed) {
                    // Si es un enlace, navegar
                    if (e.target.tagName === 'A') {
                        window.location.href = e.target.href;
                    }
                    // Si es un formulario, enviarlo
                    else if (e.target.type === 'submit') {
                        e.target.form.submit();
                    }
                }
            });
        }
    });

    // Auto-hide alerts después de 5 segundos
    document.querySelectorAll('.alert:not(.alert-permanent)').forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Inicializar componentes específicos de la página
    if (typeof window.pageInit === 'function') {
        window.pageInit();
    }
});

// Exponer utilidades globalmente
window.Utils = Utils;
window.Toast = Toast;
window.ApiClient = ApiClient;
window.FormHandler = FormHandler;
window.ModalManager = ModalManager;
