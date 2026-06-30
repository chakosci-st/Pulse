function showInitial(img, initial) {
    // Remove the broken image
    img.style.display = 'none';
    // Create the initial element
    const initialDiv = document.createElement('div');
    initialDiv.className = 'profile-initial';
    initialDiv.textContent = initial;
    // Insert after the image
    img.parentNode.appendChild(initialDiv);
}



// Function to fetch a single page of plants
function fetchPlantsPage(request) {
    return $.ajax({
        "url": getApiRootPath() + "/api/plants/datatables",
        "type": "POST",
        "contentType": "application/json",
        "data": JSON.stringify(request),
        success: function (response) {
 
        },
        error: function (xhr, status, error) {
 
        }
    });

}





// Function to fetch all plants by paging through the API
function fetchAllPlants(start, pageSize, searchvalue, status, onComplete) {
    var allPlants = [];
    var draw = 1;

    function fetchNextPage() {
        var request = {
            draw: draw++,
            start: start,
            length: pageSize,
            search: { value: searchvalue },
            isActive: status
        };

        fetchPlantsPage(request).done(function (response) {
            if (response && response.data && response.data.length > 0) {
                allPlants = allPlants.concat(response.data);
                start += pageSize;

                if (allPlants.length < response.recordsTotal) {
                    fetchNextPage(); // Fetch next page
                } else {
                    onComplete(allPlants); // All done
                }
            } else {
                onComplete(allPlants); // No more data
            }
        }).fail(function (xhr, status, error) {
            console.error("API call failed:", error);
            onComplete(allPlants); // Return what we have so far
        });
    }

    fetchNextPage();
}

// Usage example: fetch all plants and loop through them
$(document).ready(function () {
    
    var start = 0;
    var pageSize = -1; 
    var searchvalue = "";
    var status = "";


    fetchAllPlants(start, pageSize, searchvalue, status, function (allPlants) {

        appPath = getAppRootPath();

        allPlants.forEach(function (plant) {
            var bgColor = plant.isActive ? "text-bg-success" : "text-bg-danger";
            console.log(plant)

            var plantHTML = `
    <!--begin::Col-->
    <div class="col-lg-4 col-md-6 col-12 my-2">
        <div class="card card-widget widget-user shadow-lg ${bgColor} shadow">
            <!-- Add the bg color to the header using any of the bg-* classes -->
            <div class="widget-user-header text-white" style="background: url('${appPath}/content/uploads/plantfiles/${plant.plantCode}.jpg') center center;">
 <!-- Control Button -->
    <a href='/sites/plants/display/code=${plant.plantCode}' class="btn btn-light btn-sm position-absolute top-0 end-0 m-2" data-tippy-content="Edit" title="Edit">
        <i class="bi bi-gear"></i> <!-- Bootstrap Icons example -->
    </a>
                <div class="  my-5 mx-3">
                    <div   style="text-shadow: 2px 2px 4px #000000;">
                        <h1>${plant.plantName} <br />
                              <h3>${plant.plantCode}</h3>
                        </h1>      
                    </div> 
                </div>
                <div style="background:rgba(255,255,255,0.8); padding:10px">
                    <div id="${plant.plantCode}-avatar-group-container"></div>
                </div>
            </div>
            <div class="card-footer">
                <div class="row">
                    <div class="col-sm-3 border-right text-center">
                        <div class="description-block">
                            <h5 class="description-header">${plant.activeProjectsCount}</h5>
                            <span class="description-text">ACTIVE PROJECTS</span>
                        </div>
                        <!-- /.description-block -->
                    </div>
                    <!-- /.col -->
                    <div class="col-sm-3 border-right text-center">
                        <div class="description-block">
                            <h5 class="description-header">${plant.activeTasksCount}</h5>
                            <span class="description-text">ACTIVE TASKS</span>
                        </div>
                        <!-- /.description-block -->
                    </div>
                    <!-- /.col -->
                    <div class="col-sm-3 border-right text-center">
                        <div class="description-block">
                            <h5 class="description-header">${plant.taskDueCount}</h5>
                            <span class="description-text">DUE TASKS</span>
                        </div>
                        <!-- /.description-block -->
                    </div>
                    <!-- /.col -->
                    <div class="col-sm-3">
                        <div class="description-block text-center">
                            <h5 class="description-header">${plant.productCount}</h5>
                            <span class="description-text">PRODUCTS</span>
                        </div>
                        <!-- /.description-block -->
                    </div>
                    <!-- /.col -->
                </div>
                <!-- /.row -->
            </div>
        </div>
    </div>
    <!--end::Col-->
`
            $('#containerAdd').before(plantHTML);

            const dataSource = [
                { id: 1, name: "Jane Doe", initials: "JD", avatarUrl: "https://randomuser.me/api/portraits/women/1.jpg" },
                { id: 2, name: "John Smith", initials: "JS", avatarUrl: "https://randomuser.me/api/portraits/men/1.jpg" },
                { id: 3, name: "Alexandria", initials: "ALX" },
                { id: 4, name: "Charlie Clark", initials: "CCC" },
                { id: 5, name: "Emily Evans", initials: "EE", avatarUrl: "https://randomuser.me/api/portraits/women/2.jpg" },
                { id: 6, name: "Frank Foster", initials: "FF", avatarUrl: "https://randomuser.me/api/portraits/men/2.jpg" },
                { id: 7, name: "Grace Green", initials: "GG" },
                { id: 8, name: "Hannah Harris", initials: "HH", avatarUrl: "https://randomuser.me/api/portraits/women/3.jpg" },
                { id: 9, name: "Ian Irving", initials: "II" },
                { id: 10, name: "Julia Jones", initials: "JJ", avatarUrl: "https://randomuser.me/api/portraits/women/4.jpg" },
                { id: 11, name: "Kevin King", initials: "KK" },
                { id: 12, name: "Laura Lee", initials: "LL", avatarUrl: "https://randomuser.me/api/portraits/women/5.jpg" },
                { id: 13, name: "Mike Miller", initials: "MM", avatarUrl: "https://randomuser.me/api/portraits/men/3.jpg" },
                { id: 14, name: "Nina Nelson", initials: "NN" },
                { id: 15, name: "Oscar Owens", initials: "OO", avatarUrl: "https://randomuser.me/api/portraits/men/4.jpg" },
                { id: 16, name: "Paula Price", initials: "PP" },
                { id: 17, name: "Quentin Quinn", initials: "QQ" },
                { id: 18, name: "Rachel Reed", initials: "RR", avatarUrl: "https://randomuser.me/api/portraits/women/6.jpg" },
                { id: 19, name: "Steve Smith", initials: "SS", avatarUrl: "https://randomuser.me/api/portraits/men/5.jpg" },
                { id: 20, name: "Tina Turner", initials: "TT" }
            ];

            loadAndRenderAvatarGroup({
                dataSource: dataSource,
                container: `#${plant.plantCode}-avatar-group-container`,
                maxVisible: 10,
                avatarSize: 40,
                avatarSpacing: 20,
                label: 'Member',
                emptyText: 'No members found',
                backgroundColor: '#e0f2fe',
                fontColor: '#075985',
                labelBackgroundColor: '#607d8b', // blue-grey for "+N"
                userInformationUrl: '/Settings/Profile/Index/{id}',
                userInformationTarget: '_self',
                labelFontColor: '#fff',
                // Sort alphabetically by initials
                sort: 'initials',
                //// Descending by name
                //sort: function(a, b) { return b.name.localeCompare(a.name); }, 
                //// Sorting by date (asc)
                //sort: function (a, b) {
                //    // Convert to Date objects for comparison
                //    return new Date(a.registeredDate) - new Date(b.registeredDate);
                //},
                //// Sorting by date (desc)
                //sort: function (a, b) {
                //    return new Date(b.registeredDate) - new Date(a.registeredDate);
                //},
                onMoreClick: function (extraUsers, event) {
                    alert('Show more users:\n' + extraUsers.map(u => u.name).join(', '));
                    // You can open a modal, navigate, etc.
                },
                onLabelClick: function (allUsers, event) {
                    alert('All users:\n' + allUsers.map(u => u.name).join(', '));
                    // You can open a modal, navigate, etc.
                }
            });


            console.log(plant); // Example: print to console

        });


        console.log("Fetched " + allPlants.length + " plants in total.");
    });
});



