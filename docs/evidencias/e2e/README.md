# Evidencias e2e (Playwright)

Vídeos de la última ejecución de `npm run e2e` contra el stack de Docker. Se regeneran con
`npm run e2e:evidence` (ver `docs/evidencias.md`).

| Grupo | Caso | Descripción | Resultado | Vídeo |
|---|---|---|---|---|
| Sesión | S1 | avisa 5 minutos antes de que expire el token | ✅ | [vídeo](S1-avisa-5-minutos-antes-de-que-expire-el-token.webm) |
| Sesión | S2 | al vencer el token cierra la sesión y lo explica | ✅ | [vídeo](S2-al-vencer-el-token-cierra-la-sesion-y-lo-explica.webm) |
| Sesión | S3 | un 401 de la API lleva al login con el aviso | ✅ | [vídeo](S3-un-401-de-la-api-lleva-al-login-con-el-aviso.webm) |
| Sesión | S4 | al recargar con una sesión ya vencida explica que expiró | ✅ | [vídeo](S4-al-recargar-con-una-sesion-ya-vencida-explica-que-expiro.webm) |
| Sesión | S5 | cerrar sesión a mano no se presenta como expiración | ✅ | [vídeo](S5-cerrar-sesion-a-mano-no-se-presenta-como-expiracion.webm) |
| Sesión | S6 | tras iniciar sesión vuelve a la ruta pedida con sus filtros | ✅ | [vídeo](S6-tras-iniciar-sesion-vuelve-a-la-ruta-pedida-con-sus-filtros.webm) |
| Listados con filtros en la URL | L1 | el buscador se vacía si el menú quita el filtro de la URL | ✅ | [vídeo](L1-el-buscador-se-vacia-si-el-menu-quita-el-filtro-de-la-url.webm) |
| Listados con filtros en la URL | L2 | los filtros sobreviven a ir al detalle y volver atrás | ✅ | [vídeo](L2-los-filtros-sobreviven-a-ir-al-detalle-y-volver-atras.webm) |
| Listados con filtros en la URL | L3 | la URL con filtros se puede abrir en otra pestaña | ✅ | [vídeo-1](L3-la-url-con-filtros-se-puede-abrir-en-otra-pestana-1.webm) · [vídeo-2](L3-la-url-con-filtros-se-puede-abrir-en-otra-pestana-2.webm) |
| Listados con filtros en la URL | L4 | al cancelar la edición vuelve a la misma página y filtros | ✅ | [vídeo](L4-al-cancelar-la-edicion-vuelve-a-la-misma-pagina-y-filtros.webm) |
| Listados con filtros en la URL | L5 | los filtros de usuarios sobreviven a recargar y se limpian | ✅ | [vídeo](L5-los-filtros-de-usuarios-sobreviven-a-recargar-y-se-limpian.webm) |
| Listados con filtros en la URL | L6 | al borrar el único registro de la última página retrocede | ✅ | [vídeo](L6-al-borrar-el-unico-registro-de-la-ultima-pagina-retrocede.webm) |
| Listados con filtros en la URL | L7 | la lista vacía ofrece crear solo cuando no hay filtros | ✅ | [vídeo](L7-la-lista-vacia-ofrece-crear-solo-cuando-no-hay-filtros.webm) |
| Listados con filtros en la URL | L8 | cambiar la fuente de entidades limpia los filtros | ✅ | [vídeo](L8-cambiar-la-fuente-de-entidades-limpia-los-filtros.webm) |
| Diálogos y cambios sin guardar | D1 | el diálogo enfoca el primer campo, atrapa el foco y bloquea el scroll | ✅ | [vídeo](D1-el-dialogo-enfoca-el-primer-campo-atrapa-el-foco-y-bloquea-e.webm) |
| Diálogos y cambios sin guardar | D2 | Escape con cambios pide confirmar y permite seguir editando | ✅ | [vídeo](D2-escape-con-cambios-pide-confirmar-y-permite-seguir-editando.webm) |
| Diálogos y cambios sin guardar | D3 | sin cambios, Escape y clic fuera cierran directo | ✅ | [vídeo](D3-sin-cambios-escape-y-clic-fuera-cierran-directo.webm) |
| Diálogos y cambios sin guardar | D4 | Cancelar con cambios pide la misma confirmación | ✅ | [vídeo](D4-cancelar-con-cambios-pide-la-misma-confirmacion.webm) |
| Páginas de alta con cambios sin guardar | D5 | salir de "Crear solicitud" con datos pide confirmar | ✅ | [vídeo](D5-salir-de-crear-solicitud-con-datos-pide-confirmar.webm) |
| Páginas de alta con cambios sin guardar | D6 | cerrar la pestaña con datos dispara el aviso del navegador | ✅ | [vídeo](D6-cerrar-la-pestana-con-datos-dispara-el-aviso-del-navegador.webm) |
| Páginas de alta con cambios sin guardar | D7 | registrar la solicitud navega al detalle sin pedir confirmación | ✅ | [vídeo](D7-registrar-la-solicitud-navega-al-detalle-sin-pedir-confirmac.webm) |
| Páginas de alta con cambios sin guardar | D8 | cambiar de fuente en "Nueva entidad" no se trata como salir | ✅ | [vídeo](D8-cambiar-de-fuente-en-nueva-entidad-no-se-trata-como-salir.webm) |
| Concurrencia entre dos usuarios | C1 | asignar con datos viejos avisa y recarga la solicitud | ✅ | [vídeo-1](C1-asignar-con-datos-viejos-avisa-y-recarga-la-solicitud-1.webm) · [vídeo-2](C1-asignar-con-datos-viejos-avisa-y-recarga-la-solicitud-2.webm) |
| Concurrencia entre dos usuarios | C2 | guardar una edición vieja conserva lo escrito y el segundo intento guarda | ✅ | [vídeo-1](C2-guardar-una-edicion-vieja-conserva-lo-escrito-y-el-segundo-i-1.webm) · [vídeo-2](C2-guardar-una-edicion-vieja-conserva-lo-escrito-y-el-segundo-i-2.webm) |
| Concurrencia entre dos usuarios | C3 | dos comentarios a la vez se guardan ambos | ✅ | [vídeo-1](C3-dos-comentarios-a-la-vez-se-guardan-ambos-1.webm) · [vídeo-2](C3-dos-comentarios-a-la-vez-se-guardan-ambos-2.webm) |
| Bandeja de notificaciones | N1 | "Ver más" carga la página siguiente de la bandeja | ✅ | [vídeo](N1-ver-mas-carga-la-pagina-siguiente-de-la-bandeja.webm) |
| Bandeja de notificaciones | N2 | "Marcar todas como leídas" deja el contador en cero | ✅ | [vídeo](N2-marcar-todas-como-leidas-deja-el-contador-en-cero.webm) |
| Bandeja de notificaciones | N3 | "Ver solicitud" abre el detalle y marca la notificación como leída | ✅ | [vídeo](N3-ver-solicitud-abre-el-detalle-y-marca-la-notificacion-como-l.webm) |
| Administración de usuarios | U1 | crea un usuario con rol Analista | ✅ | [vídeo](U1-crea-un-usuario-con-rol-analista.webm) |
| Administración de usuarios | U2 | cambia el rol de un usuario existente | ✅ | [vídeo](U2-cambia-el-rol-de-un-usuario-existente.webm) |
| Administración de usuarios | U3 | un correo repetido muestra el error y mantiene el diálogo | ✅ | [vídeo](U3-un-correo-repetido-muestra-el-error-y-mantiene-el-dialogo.webm) |
| Errores de pantalla y carga bajo demanda | E1 | una ruta inexistente muestra "Página no encontrada" con el menú | ✅ | [vídeo](E1-una-ruta-inexistente-muestra-pagina-no-encontrada-con-el-men.webm) |
| Errores de pantalla y carga bajo demanda | E2 | si no se puede descargar una pantalla muestra el error en español con el menú | ✅ | [vídeo](E2-si-no-se-puede-descargar-una-pantalla-muestra-el-error-en-es.webm) |
| Errores de pantalla y carga bajo demanda | E3 | mientras llega la pantalla muestra "Cargando pantalla…" | ✅ | [vídeo](E3-mientras-llega-la-pantalla-muestra-cargando-pantalla.webm) |
| Móvil y accesibilidad | M1 | el menú lleva el foco al abrir y Escape lo cierra devolviéndolo | ✅ | [vídeo](M1-el-menu-lleva-el-foco-al-abrir-y-escape-lo-cierra-devolviend.webm) |
| Móvil y accesibilidad | M2 | con el menú cerrado sus enlaces quedan fuera del teclado | ✅ | [vídeo](M2-con-el-menu-cerrado-sus-enlaces-quedan-fuera-del-teclado.webm) |
| Móvil y accesibilidad | M3 | inicio y bandeja caben en 375 px sin scroll horizontal | ✅ | [vídeo](M3-inicio-y-bandeja-caben-en-375-px-sin-scroll-horizontal.webm) |
| Móvil y accesibilidad | M4 | con "reducir movimiento" el menú aparece sin transición | ✅ | [vídeo](M4-con-reducir-movimiento-el-menu-aparece-sin-transicion.webm) |
| Entrega con nginx | X1 | la API se llama en el mismo origen y sin preflight CORS | ✅ | [vídeo](X1-la-api-se-llama-en-el-mismo-origen-y-sin-preflight-cors.webm) |
| Entrega con nginx | X2 | cabeceras de seguridad presentes y ninguna violación de CSP al navegar | ✅ | [vídeo](X2-cabeceras-de-seguridad-presentes-y-ninguna-violacion-de-csp.webm) |
| Entrega con nginx | X3 | la aplicación no se puede incrustar en un iframe ajeno | ✅ | [vídeo](X3-la-aplicacion-no-se-puede-incrustar-en-un-iframe-ajeno.webm) |
