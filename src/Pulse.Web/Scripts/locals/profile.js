const config = document.getElementById('profile-photo-config');
const profilePhotoUrlTemplate = config.dataset.photoUrlTemplate;

function getProfilePhotoUrl(userId, version) {
    return profilePhotoUrlTemplate
        .replace('__userid__', encodeURIComponent(userId))
        .replace('__version__', encodeURIComponent(version || '0'));
}
 

function getPhotoUrl(employeeId) {
    return photoUrlTemplate.replace('__employeeid__', encodeURIComponent(employeeId));
}