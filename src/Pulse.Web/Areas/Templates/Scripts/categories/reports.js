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



// Function to fetch a single page of categories
function fetchCategoriesPage(request) {
    return $.ajax({
        "url": getApiRootPath() + "/api/categories/datatables",
        "type": "POST",
        "contentType": "application/json",
        "data": JSON.stringify(request),
        success: function (response) {
 
        },
        error: function (xhr, status, error) {
 
        }
    });

}





// Function to fetch all categories by paging through the API
function fetchAllCategories(start, pageSize, searchvalue, status, onComplete) {
    var allCategories = [];
    var draw = 1;

    function fetchNextPage() {
        var request = {
            draw: draw++,
            start: start,
            length: pageSize,
            search: { value: searchvalue },
            isActive: status
        };

        fetchCategoriesPage(request).done(function (response) {
            if (response && response.data && response.data.length > 0) {
                allCategories = allCategories.concat(response.data);
                start += pageSize;

                if (allCategories.length < response.recordsTotal) {
                    fetchNextPage(); // Fetch next page
                } else {
                    onComplete(allCategories); // All done
                }
            } else {
                onComplete(allCategories); // No more data
            }
        }).fail(function (xhr, status, error) {
            console.error("API call failed:", error);
            onComplete(allCategories); // Return what we have so far
        });
    }

    fetchNextPage();
}

// Usage example: fetch all categories and loop through them
$(document).ready(function () {
    
    var start = 0;
    var pageSize = -1; 
    var searchvalue = "";
    var status = "";


    fetchAllCategories(start, pageSize, searchvalue, status, function (rows) {

        appPath = getAppRootPath();

        rows.forEach(function (row) {
            var categoryHTML = `
    <!--begin::Col-->
    <div class="col-lg-4 col-md-6 col-12">
        <div class="card card-widget widget-user shadow-lg text-bg-success shadow">
            <!-- Add the bg color to the header using any of the bg-* classes -->
            <div class="widget-user-header text-white">
                <div class="  my-5 mx-3">
                    <div   style="text-shadow: 2px 2px 4px #000000;">
                        <h1>${row.categoryName} <br />
                              <h3>${row.categoryCode}</h3>
                        </h1>
                    </div>
                </div>
            </div>

            <div class="card-footer">
                <div class="row">
                    <div class="col-sm-3 border-right text-center">
                        <div class="description-block">
                            <h5 class="description-header">${row.activeProjectsCount}</h5>
                            <span class="description-text">ACTIVE PROJECTS</span>
                        </div>
                        <!-- /.description-block -->
                    </div>
                    <!-- /.col -->
                    <div class="col-sm-3 border-right text-center">
                        <div class="description-block">
                            <h5 class="description-header">${row.activeTasksCount}</h5>
                            <span class="description-text">ACTIVE TASKS</span>
                        </div>
                        <!-- /.description-block -->
                    </div>
                    <!-- /.col -->
                    <div class="col-sm-3 border-right text-center">
                        <div class="description-block">
                            <h5 class="description-header">${row.taskDueCount}</h5>
                            <span class="description-text">DUE TASKS</span>
                        </div>
                        <!-- /.description-block -->
                    </div>
                    <!-- /.col -->
                    <div class="col-sm-3">
                        <div class="description-block text-center">
                            <h5 class="description-header">${row.productCount}</h5>
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
            $('#containerAdd').before(categoryHTML);

             

        });


        console.log("Fetched " + rows.length + " categories in total.");
    });
});



