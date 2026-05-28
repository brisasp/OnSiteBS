window.firmarAcuerdo = {
    makeDraggable: function (panelId, handleId) {
        const panel = document.getElementById(panelId);
        const handle = document.getElementById(handleId);

        if (!panel || !handle) return;

        let isDragging = false;
        let offsetX = 0;
        let offsetY = 0;

        handle.onmousedown = function (e) {
            isDragging = true;

            const rect = panel.getBoundingClientRect();
            offsetX = e.clientX - rect.left;
            offsetY = e.clientY - rect.top;

            panel.style.right = "auto";
            panel.style.bottom = "auto";
            panel.style.left = rect.left + "px";
            panel.style.top = rect.top + "px";

            document.onmousemove = function (moveEvent) {
                if (!isDragging) return;

                panel.style.left = (moveEvent.clientX - offsetX) + "px";
                panel.style.top = (moveEvent.clientY - offsetY) + "px";
            };

            document.onmouseup = function () {
                isDragging = false;
                document.onmousemove = null;
                document.onmouseup = null;
            };
        };
    }
};