

(function () {

    var backdrop = document.getElementById('drawerBackdrop');
    var btnOpen = document.getElementById('btnRegisterProject');
    var btnClose = document.getElementById('btnCloseDrawer');
    var btnCancel = document.getElementById('btnCancelDrawer');
    const activeprojects = document.getElementById('activeprojects');

    function openDrawer() {
        

        $('#drawerContent').html(`
<!-- Side Drawer -->
    <aside class="drawer" id="drawerRegisterProject" aria-hidden="true">
        <div class="drawer-header">
            <h2 class="drawer-title">Start Project</h2>
            <button class="drawer-close" type="button" aria-label="Close" id="btnCloseDrawer">&times;</button>
        </div>
        <div class="drawer-body">
            <small class="text-muted d-block mb-3">
                Choose the products and roadmap for the new Project or link on existing project.
            </small>
            <div class="form-group">
                <label for="nrProjectName" class="small font-weight-semibold">Project Name</label>
                <input type="text" class="form-control" id="nrProjectName" required />
            </div>

            <div class="form-group">
                <label for="nrCategory" class="small font-weight-semibold">Project Description</label>
                <textarea class="form-control" id="nrProjectDescription"></textarea>
            </div>
        </div>
        <div class="drawer-footer">
            <button type="button" class="btn btn-outline-secondary btn-sm mr-2" id="btnCancelDrawer">
                Cancel
            </button>
            <button type="submit" form="formNewRequest" class="btn btn-primary btn-sm">
                Start Request
            </button>
        </div>
    </aside>

`);

        var drawer = document.getElementById('drawerRegisterProject');

        drawer.classList.add('open');
        backdrop.classList.add('show');
        drawer.setAttribute('aria-hidden', 'false');
        document.body.style.overflow = 'hidden';
        // focus first field
        setTimeout(function () {
            var sel = document.getElementById('nrCategory');
            if (sel) sel.focus();
        }, 100);
    }

    function closeDrawer() {
        drawer.classList.remove('open');
        backdrop.classList.remove('show');
        drawer.setAttribute('aria-hidden', 'true');
        document.body.style.overflow = '';
    }

    if (btnOpen) btnOpen.addEventListener('click', openDrawer);
    if (btnClose) btnClose.addEventListener('click', closeDrawer);
    if (btnCancel) btnCancel.addEventListener('click', closeDrawer);
    if (backdrop) backdrop.addEventListener('click', closeDrawer);

    // ESC key closes drawer
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && drawer.classList.contains('open')) {
            closeDrawer();
        }
    });

    // Simple submit handler demo
    document.getElementById('formRegisterProject').addEventListener('submit', function (e) {
        e.preventDefault();
        // Here you would redirect or call your back‑end to create the request
        alert('Start request for: '
            + document.getElementById('nrCategory').value + ' / '
            + document.getElementById('nrForm').value + ' / '
            + document.getElementById('nrSite').value);
        closeDrawer();
    });





})();