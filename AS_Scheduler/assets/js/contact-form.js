var $ = jQuery.noConflict();

$(document).ready(function($) {
	"use strict";
	
    /* ---------------------------------------------------------------------- */
    /*  Contact Form
    /* ---------------------------------------------------------------------- */

	var res = {
	    loader: $('<div />', { class: 'loader' }),
        container: $('.ajaxContainer')
	}

    var submitContact = $('#submit_contact'),
        message = $('#msg');

    submitContact.on('click', function(e){
        e.preventDefault();

        var $this = $(this);

        var errorOptions = {
            rules: {
                // no quoting necessary
                name: {
                    required: true
                },
                email: {
                    required: true
                },
                subject: {
                    required: true
                },
                message: {
                    required: true
                }
            },
            messages: {
                name: "Name is required.",
                email: "Email is required.",
                subject: "Subject is required.",
                message: "Message is required."
            }
        };

        var validator = $("#contact-form").validate(errorOptions);
        if (!validator.form()) {
            return false;
        }
        $.ajax({
            type: "POST",
            url: $('#contact-form').attr('action'),
            dataType: 'json',
            cache: false,
            data: $('#contact-form').serialize(),
            beforeSend: function () {
                res.container.append(res.loader);
            },
            success: function (data) {
                if (data.Message === "Success! " + "<i class='fa fa-check'></i>") {
                    $this.parents('form').find('input[type=text],textarea,select').filter(':visible').val('');
                    message.hide().removeClass('success').removeClass('error').addClass('success').html(data.Message).fadeIn('slow').delay(5000).fadeOut('slow');
                } else {
                    message.hide().removeClass('success').removeClass('error').addClass('error').html(data.Message).fadeIn('slow').delay(5000).fadeOut('slow');
                }
                res.container.find(res.loader).remove();
            }
        });
    });

    ///* ---------------------------------------------------------------------- */
    ///*  Contact Map
    ///* ---------------------------------------------------------------------- */

    
    //var myMarkers = {
    //    "markers": [
    //        {
    //            "latitude": "36.050727",       // latitude
    //            "longitude":"-79.841125",       // longitude
    //            "icon": "/assets/upload/map_pin_1.png"  // Pin icon
    //        }
             

    //        // Add As Plenty As u want
    //        // , {
    //        //     "latitude": "40.712785",
    //        //     "longitude":"-73.969708",
    //        //     "icon": "~/assets/upload/map_pin_1.png"
    //        // }

            
    //    ]
    //};

    //try {
    //    $("#google_map").mapmarker({
    //        zoom : 14,                          // Zoom
    //        center: "36.050727,-79.841125",        // Center of map
    //        type: "ROADMAP",                    // Map Type
    //        controls: "HORIZONTAL_BAR",         // Controls style
    //        dragging:1,                         // Allow dragging?
    //        mousewheel:0,                       // Allow zooming with mousewheel
    //        markers: myMarkers,
    //        styling: 0,                         // Bool - do you want to style the map?
    //        featureType:"all",
    //        visibility: "on",
    //        elementType:"geometry",
    //        hue:"#00AAFF",
    //        saturation:-100,
    //        lightness:0,
    //        gamma:1,
    //        navigation_control:0
    //        /*
    //        To play with the map colors and styles you can try this tool right here http://gmaps-samples-v3.googlecode.com/svn/trunk/styledmaps/wizard/index.html
    //        */
    //    });

    //} catch (err) {
    //    //console.log(err);
    //}
});
