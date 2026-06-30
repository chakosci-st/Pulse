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

        var appPath = getAppRootPath();
        var apiPath = getApiRootPath();

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
    <a href='${appPath}/templates/plants/Overview?code=${plant.plantCode}' class="btn btn-light btn-sm position-absolute top-0 end-0 m-2" data-tippy-content="Overview" title="Overview">
        <i class="bi bi-gear"></i>
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



            loadAndRenderAvatarGroup({
                dataSource: {
                    url: apiPath + '/api/plantmembers/PerPlant',
                    method: 'GET',
                    data: { code: plant.plantCode }
                },
                container: `#${plant.plantCode}-avatar-group-container`,
                maxVisible: 10,
                avatarSize: 40,
                avatarSpacing: 20,
                label: 'Member',
                emptyText: 'No members found',
                backgroundColor: '#FFC107', // orange
                fontColor: '#222',          // dark text
                labelBackgroundColor: '#607d8b', // blue-grey for "+N"
                userInformationUrl: '/home/index/{id}', // or user => `/profile/${user.id}`
                userInformationTarget: '_blank',   // or '_self'
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
                },
                transform: function (data) {
                    return data.map(function (item) {
                        return {
                            id: item.userId,
                            name: item.userInfo.firstName + " " + item.userInfo.lastName,
                            avatarUrl: "https://calwebapps.cal.st.com/ProfilePhoto/" + item.userId + ".jpg"
                        };
                    });
                },
            });



        });


        //console.log("Fetched " + allPlants.length + " plants in total.");
    });
});



