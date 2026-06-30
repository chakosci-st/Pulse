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
        "url": "http://localhost:51673/api/plants/datatables",
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
        
        allPlants.forEach(function (plant) {
            var plantHTML = `
    <!--begin::Col-->
    <div class="col-lg-4 col-md-6 col-12">
        <div class="card card-widget widget-user shadow-lg text-bg-success shadow">
            <!-- Add the bg color to the header using any of the bg-* classes -->
            <div class="widget-user-header text-white" style="background: url('/uploads/plant/${plant.plantCode}.png') center center;">
                <div class="  my-5 mx-3">
                    <div   style="text-shadow: 2px 2px 4px #000000;">
                        <h1>${plant.plantName} <br />
                              <h3>${plant.plantCode}</h3>
                        </h1>
                    </div>
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


            console.log(plant); // Example: print to console

        });


        console.log("Fetched " + allPlants.length + " plants in total.");
    });
});



