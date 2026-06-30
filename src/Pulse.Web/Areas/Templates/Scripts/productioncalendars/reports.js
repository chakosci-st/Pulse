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



// Function to fetch a single page of productioncalendars
function fetchPageRows(request) {
    return $.ajax({
        "url": getApiRootPath() + "/api/productioncalendars/datatables",
        "type": "POST",
        "contentType": "application/json",
        "data": JSON.stringify(request),
        success: function (response) {
 
        },
        error: function (xhr, status, error) {
 
        }
    });

}





// Function to fetch all productioncalendars by paging through the API
function fetchAllRows(start, pageSize, searchvalue, status, onComplete) {
    var rows = [];
    var draw = 1;

    function fetchNextPage() {
        var request = {
            draw: draw++,
            start: start,
            length: pageSize,
            search: { value: searchvalue },
            isActive: status
        };

        fetchPageRows(request).done(function (response) {
            if (response && response.data && response.data.length > 0) {
                rows = rows.concat(response.data);
                start += pageSize;

                if (rows.length < response.recordsTotal) {
                    fetchNextPage(); // Fetch next page
                } else {
                    onComplete(rows); // All done
                }
            } else {
                onComplete(rows); // No more data
            }
        }).fail(function (xhr, status, error) {
            console.error("API call failed:", error);
            onComplete(rows); // Return what we have so far
        });
    }

    fetchNextPage();
}

// Usage example: fetch all productioncalendars and loop through them
$(document).ready(function () {
    
    var start = 0;
    var pageSize = -1; 
    var searchvalue = "";
    var status = "";


    fetchAllRows(start, pageSize, searchvalue, status, function (rows) {

        appPath = getAppRootPath();

        rows.forEach(function (row) {
            var rowHTML = `
    <!--begin::Col-->
    <div class="col-lg-4 col-md-6 col-12">
        <div class="card card-widget widget-user shadow-lg text-bg-success shadow">
            <!-- Add the bg color to the header using any of the bg-* classes -->
            <div class="widget-user-header text-white" >
                <div class="  my-5 mx-3">
                    <div   style="text-shadow: 2px 2px 4px #000000;">
                        <h1>${row.calendarYear} <br /> 
      <h3></h3>
                        </h1>
                    </div>
                </div>
            </div>

            <div class="card-footer">
                <div class="row">
                    <div class="col-sm-3 border-right text-center">
                        <div class="description-block">
                            <h5 class="description-header"></h5>
                            <span class="description-text"></span>
                        </div>
                        <!-- /.description-block -->
                    </div>
                    <!-- /.col -->
                    <div class="col-sm-3 border-right text-center">
                        <div class="description-block">
                            <h5 class="description-header"></h5>
                            <span class="description-text"></span>
                        </div>
                        <!-- /.description-block -->
                    </div>
                    <!-- /.col -->
                    <div class="col-sm-3 border-right text-center">
                        <div class="description-block">
                            <h5 class="description-header"></h5>
                            <span class="description-text"></span>
                        </div>
                        <!-- /.description-block -->
                    </div>
                    <!-- /.col -->
                    <div class="col-sm-3">
                        <div class="description-block text-center">
                            <h5 class="description-header"></h5>
                            <span class="description-text"></span>
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
            $('#containerAdd').before(rowHTML);

             

        });


        console.log("Fetched " + rows.length + " productioncalendars in total.");
    });
});



