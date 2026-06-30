var usersReportRows = [];

function escapeHtml(value) {
	return String(value == null ? '' : value)
		.replace(/&/g, '&amp;')
		.replace(/</g, '&lt;')
		.replace(/>/g, '&gt;')
		.replace(/"/g, '&quot;')
		.replace(/'/g, '&#39;');
}

function parseDisplayName(row) {
	var name = ((row.firstName || '') + ' ' + (row.lastName || '')).trim();
	if (name) {
		return name;
	}

	return row.userName || row.userId || 'Unknown User';
}

function normalizeText(value) {
	return String(value || '').trim().toLowerCase();
}

function toNumber(value) {
	var parsed = Number(value);
	return Number.isFinite(parsed) ? parsed : 0;
}

function showAvatarFallback(imgElement) {
	var $img = $(imgElement);
	$img.hide();
	$img.siblings('.user-avatar__initials').css('display', 'inline-flex');
}

function buildCardHtml(row) {
	var displayName = escapeHtml(parseDisplayName(row));
	var userName = escapeHtml(row.userName || row.userId || '-');
	var userId = escapeHtml(row.userId || '-');
	var email = escapeHtml(row.email || '-');
	var initials = escapeHtml(row.initials || '?');
	var photoUrl = escapeHtml(row.photoUrl || '');
	var isActive = !!row.isActive;
	var statusClass = isActive ? 'user-status-pill user-status-pill--active' : 'user-status-pill user-status-pill--inactive';
	var statusLabel = isActive ? 'Active' : 'Inactive';

	var stats = row.stats || {};
	var projectsOwned = toNumber(stats.projectsOwned);
	var projectsAsMember = toNumber(stats.projectsAsMember);
	var pendingTasks = toNumber(stats.pendingTasks);

	return '' +
		'<article class="user-report-card" data-user-id="' + userId + '">' +
		'  <div class="user-report-card__header">' +
		'    <div class="user-report-profile">' +
		(photoUrl
			? '      <img class="user-avatar" src="' + photoUrl + '" alt="' + displayName + '" onerror="showAvatarFallback(this)">' 
			: '      <img class="user-avatar" src="" alt="" style="display:none;">') +
		'      <span class="user-avatar__initials" style="' + (photoUrl ? 'display:none;' : 'display:inline-flex;') + '">' + initials + '</span>' +
		'      <div class="user-meta">' +
		'        <p class="user-meta__name" title="' + displayName + '">' + displayName + '</p>' +
		'        <p class="user-meta__sub" title="' + userName + '">' + userName + ' | ' + userId + '</p>' +
		'        <p class="user-meta__sub" title="' + email + '">' + email + '</p>' +
		'      </div>' +
		'    </div>' +
		'    <span class="' + statusClass + '">' + statusLabel + '</span>' +
		'  </div>' +
		'  <div class="user-report-stats">' +
		'    <div class="user-stat">' +
		'      <span class="user-stat__value">' + projectsOwned + '</span>' +
		'      <span class="user-stat__label">Projects Owned</span>' +
		'    </div>' +
		'    <div class="user-stat">' +
		'      <span class="user-stat__value">' + projectsAsMember + '</span>' +
		'      <span class="user-stat__label">Projects as Member</span>' +
		'    </div>' +
		'    <div class="user-stat">' +
		'      <span class="user-stat__value">' + pendingTasks + '</span>' +
		'      <span class="user-stat__label">Pending Tasks</span>' +
		'    </div>' +
		'  </div>' +
		'</article>';
}

function applyFiltersAndRender() {
	var searchValue = normalizeText($('#userCardSearch').val());
	var statusFilter = normalizeText($('#userCardStatusFilter').val());

	var filtered = usersReportRows.filter(function (row) {
		var rowStatus = row.isActive ? 'active' : 'inactive';
		if (statusFilter && rowStatus !== statusFilter) {
			return false;
		}

		if (!searchValue) {
			return true;
		}

		var haystack = [
			row.firstName,
			row.lastName,
			row.userName,
			row.userId,
			row.email
		].map(normalizeText).join(' ');

		return haystack.indexOf(searchValue) >= 0;
	});

	var html = filtered.map(buildCardHtml).join('');
	$('#usersReportGrid').html(html);
	$('#usersReportEmpty').toggle(filtered.length === 0);
}

function fetchUserCards() {
	return $.ajax({
		url: getApiRootPath() + '/api/users/reports/cards',
		type: 'GET',
		contentType: 'application/json'
	});
}

$(document).ready(function () {
	fetchUserCards().done(function (response) {
		usersReportRows = (response && response.data) ? response.data : [];
		applyFiltersAndRender();
	}).fail(function () {
		toastr.error('Unable to load user report cards.');
	});

	$('#userCardSearch').on('input', applyFiltersAndRender);
	$('#userCardStatusFilter').on('change', applyFiltersAndRender);
});
