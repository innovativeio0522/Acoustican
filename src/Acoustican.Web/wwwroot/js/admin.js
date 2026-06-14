const API_URL = '/api';

// Retrieve token helper
function getAdminToken() {
    return localStorage.getItem('adminToken');
}

// Check auth state
function checkAdminAuth() {
    // Admin token is only used to gate /admin/* routes.
    const token = getAdminToken();

    // Only enforce when we are actually on an admin route.
    const isAdminRoute = window.location.pathname === '/admin' || window.location.pathname.startsWith('/admin/');
    if (!isAdminRoute) return;

    if (!token) {
        window.location.href = '/admin';
    }
}


// Handle login
async function handleLogin(event) {

    event.preventDefault();
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;

    try {
        const response = await fetch(API_URL + '/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        }).then(r => r.json());

        if (response.success) {
            localStorage.setItem('adminToken', response.token);
            localStorage.setItem('userToken', response.token);
            localStorage.setItem('userData', JSON.stringify(response.user));

            // Role-based redirect after successful login
            // - Admin -> admin dashboard
            // - Non-admin -> normal landing page (/)
            if (response.user?.role === 'Admin') {
                window.location.href = '/admin/dashboard';
            } else {
                window.location.href = '/';
            }
        } else {
            document.getElementById('loginError').innerHTML = `<div class="alert alert-danger">${response.message}</div>`;
        }
    } catch (err) {
        console.error('Login error:', err);
        document.getElementById('loginError').innerHTML = '<div class="alert alert-danger">Connection error. Please try again.</div>';
    }
}

// Handle logout
function logout() {
    localStorage.removeItem('adminToken');
    localStorage.removeItem('userToken');
    localStorage.removeItem('userData');
    window.location.href = '/admin';
}

// Load Dashboard
async function loadDashboard() {
    const token = getAdminToken();
    if (!token) return;
    try {
        const courses = await fetch(API_URL + '/courses', {
            headers: { 'Authorization': `Bearer ${token}` }
        }).then(r => r.json());
        
        const testimonials = await fetch(API_URL + '/testimonials', {
            headers: { 'Authorization': `Bearer ${token}` }
        }).then(r => r.json());
        
        const pricing = await fetch(API_URL + '/pricing', {
            headers: { 'Authorization': `Bearer ${token}` }
        }).then(r => r.json());

        const statCoursesEl = document.getElementById('statCourses');
        const statPublishedEl = document.getElementById('statPublished');
        const statTestimonialsEl = document.getElementById('statTestimonials');
        const statPricingEl = document.getElementById('statPricing');

        if (statCoursesEl) statCoursesEl.textContent = courses.length;
        if (statPublishedEl) statPublishedEl.textContent = courses.filter(c => c.isPublished).length;
        if (statTestimonialsEl) statTestimonialsEl.textContent = testimonials.length;
        if (statPricingEl) statPricingEl.textContent = pricing.length;
    } catch (err) {
        console.error('Error loading dashboard:', err);
    }
}

// Load Hero Section Content
async function loadHero() {
    const token = getAdminToken();
    try {
        const hero = await fetch(`${API_URL}/hero`).then(r => r.json());
        document.getElementById('heroId').value = hero.id;
        document.getElementById('heroTitle').value = hero.title;
        document.getElementById('heroSubtitle').value = hero.subtitle;
        document.getElementById('heroDescription').value = hero.description;
        document.getElementById('heroVideoUrl').value = hero.backgroundVideoUrl || '';
        document.getElementById('heroImageUrl').value = hero.backgroundImageUrl || '';
        document.getElementById('heroPreviewVideoId').value = hero.previewVideoId || '';
        document.getElementById('heroPrimaryBtn').value = hero.primaryButtonText;
        document.getElementById('heroSecondaryBtn').value = hero.secondaryButtonText;

        // Setup auto-upload for hero files
        document.getElementById('heroVideoFile').onchange = async (e) => {
            const file = e.target.files[0];
            if (!file) return;
            const formData = new FormData();
            formData.append('file', file);
            const response = await fetch(`${API_URL}/files/upload-video`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${token}` },
                body: formData
            }).then(r => r.json());
            if (response.success) document.getElementById('heroVideoUrl').value = response.filePath;
        };

        document.getElementById('heroImageFile').onchange = async (e) => {
            const file = e.target.files[0];
            if (!file) return;
            const formData = new FormData();
            formData.append('file', file);
            const response = await fetch(`${API_URL}/files/upload-image`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${token}` },
                body: formData
            }).then(r => r.json());
            if (response.success) document.getElementById('heroImageUrl').value = response.filePath;
        };
    } catch (err) {
        console.error('Error loading hero content:', err);
    }
}

// Save Hero Content
async function saveHero(event) {
    event.preventDefault();
    const token = getAdminToken();
    const heroData = {
        id: parseInt(document.getElementById('heroId').value),
        title: document.getElementById('heroTitle').value,
        subtitle: document.getElementById('heroSubtitle').value,
        description: document.getElementById('heroDescription').value,
        backgroundVideoUrl: document.getElementById('heroVideoUrl').value,
        backgroundImageUrl: document.getElementById('heroImageUrl').value,
        previewVideoId: document.getElementById('heroPreviewVideoId').value,
        primaryButtonText: document.getElementById('heroPrimaryBtn').value,
        secondaryButtonText: document.getElementById('heroSecondaryBtn').value,
        isActive: true
    };

    try {
        const response = await fetch(`${API_URL}/hero`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(heroData)
        });

        if (response.ok) {
            alert('Hero content updated successfully!');
        } else {
            alert('Error updating hero content');
        }
    } catch (err) {
        console.error('Error saving hero:', err);
    }
}

// Load Courses list
async function loadCourses() {
    const token = getAdminToken();
    try {
        const courses = await fetch(API_URL + '/courses', {
            headers: { 'Authorization': `Bearer ${token}` }
        }).then(r => r.json());

        const table = document.getElementById('coursesTable');
        table.innerHTML = courses.map(c => `
            <tr>
                <td>${c.title}</td>
                <td><span class="badge bg-info">${c.level}</span></td>
                <td>$${c.price}</td>
                <td>${c.durationMinutes} min</td>
                <td>${c.isPublished ? '<span class="badge bg-success">Published</span>' : '<span class="badge bg-secondary">Draft</span>'}</td>
                <td>
                    <button class="btn btn-sm btn-warning" onclick="editCourse(${c.id})">Edit</button>
                    <button class="btn btn-sm btn-danger" onclick="deleteCourse(${c.id})">Delete</button>
                </td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading courses:', err);
    }
}

// Show course add/edit form modal
let courseModal;
function showCourseForm(course = null) {
    if (!courseModal) courseModal = new bootstrap.Modal(document.getElementById('courseModal'));
    
    const form = document.getElementById('courseForm');
    form.reset();
    
    // Handle file upload preview/selection
    document.getElementById('courseThumbnailFile').onchange = async (e) => {
        const file = e.target.files[0];
        if (!file) return;

        const formData = new FormData();
        formData.append('file', file);

        const endpoint = file.type.startsWith('video/') ? '/files/upload-video' : '/files/upload-image';
        const token = getAdminToken();
        
        try {
            const response = await fetch(`${API_URL}${endpoint}`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${token}` },
                body: formData
            }).then(r => r.json());

            if (response.success) {
                document.getElementById('courseThumbnailUrl').value = response.filePath;
            } else {
                alert('Upload failed: ' + response.message);
            }
        } catch (err) {
            console.error('Upload error:', err);
            alert('Upload error');
        }
    };
    
    if (course) {
        document.getElementById('courseModalTitle').textContent = 'Edit Course';
        document.getElementById('courseId').value = course.id;
        document.getElementById('courseTitle').value = course.title;
        document.getElementById('courseThumbnailUrl').value = course.thumbnailUrl || '';
        document.getElementById('courseDescription').value = course.description;
        document.getElementById('courseLevel').value = course.level;
        document.getElementById('coursePrice').value = course.price;
        document.getElementById('courseOriginalPrice').value = course.originalPrice || 0;
        document.getElementById('courseDuration').value = course.durationMinutes;
        document.getElementById('courseLectureCount').value = course.lectureCount || 0;
        document.getElementById('courseInstructorName').value = course.instructorName || '';
        document.getElementById('courseRating').value = course.rating || 0;
        document.getElementById('courseReviewCount').value = course.reviewCount || 0;
        document.getElementById('courseStudentCount').value = course.studentCount || 0;
        document.getElementById('courseIsBestseller').checked = course.isBestseller;
        document.getElementById('coursePublished').checked = course.isPublished;
        document.getElementById('courseWhatYoullLearn').value = course.whatYoullLearn ? course.whatYoullLearn.split(',').join('\n') : '';
        document.getElementById('courseRequirements').value = course.requirements ? course.requirements.split(',').join('\n') : '';
    } else {
        document.getElementById('courseModalTitle').textContent = 'Add Course';
        document.getElementById('courseId').value = '';
    }
    
    courseModal.show();
}

// Save Course
async function saveCourse() {
    const id = document.getElementById('courseId').value;
    const token = getAdminToken();
    const courseData = {
        title: document.getElementById('courseTitle').value,
        description: document.getElementById('courseDescription').value,
        thumbnailUrl: document.getElementById('courseThumbnailUrl').value,
        level: document.getElementById('courseLevel').value,
        price: parseFloat(document.getElementById('coursePrice').value),
        originalPrice: parseFloat(document.getElementById('courseOriginalPrice').value) || 0,
        durationMinutes: parseInt(document.getElementById('courseDuration').value),
        lectureCount: parseInt(document.getElementById('courseLectureCount').value) || 0,
        instructorName: document.getElementById('courseInstructorName').value,
        studentCount: parseInt(document.getElementById('courseStudentCount').value) || 0,
        rating: parseFloat(document.getElementById('courseRating').value) || 0,
        reviewCount: parseInt(document.getElementById('courseReviewCount').value) || 0,
        isBestseller: document.getElementById('courseIsBestseller').checked,
        isPublished: document.getElementById('coursePublished').checked,
        whatYoullLearn: document.getElementById('courseWhatYoullLearn').value.split('\n').filter(f => f.trim() !== '').join(','),
        requirements: document.getElementById('courseRequirements').value.split('\n').filter(f => f.trim() !== '').join(',')
    };

    const method = id ? 'PUT' : 'POST';
    const url = id ? `${API_URL}/courses/${id}` : `${API_URL}/courses`;

    try {
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(courseData)
        });

        if (response.ok) {
            courseModal.hide();
            loadCourses();
        } else {
            const err = await response.json();
            alert('Error saving course: ' + (err.message || 'Unknown error'));
        }
    } catch (err) {
        console.error('Error saving course:', err);
        alert('Error saving course');
    }
}

// Edit Course load details
async function editCourse(id) {
    const token = getAdminToken();
    try {
        const course = await fetch(`${API_URL}/courses/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        }).then(r => r.json());
        showCourseForm(course);
    } catch (err) {
        console.error('Error fetching course:', err);
    }
}

// Delete Course
async function deleteCourse(id) {
    const token = getAdminToken();
    if (!confirm('Are you sure you want to delete this course?')) return;

    try {
        const response = await fetch(`${API_URL}/courses/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            loadCourses();
        } else {
            alert('Error deleting course');
        }
    } catch (err) {
        console.error('Error deleting course:', err);
    }
}

// Populate Course filter/dropdowns for modules
async function populateCourseDropdowns() {
    try {
        const courses = await fetch(API_URL + '/courses').then(r => r.json());
        const filter = document.getElementById('moduleCourseFilter');
        const modalSelect = document.getElementById('moduleCourseId');
        
        const options = courses.map(c => `<option value="${c.id}">${c.title}</option>`).join('');
        
        if (filter) filter.innerHTML = '<option value="">Select a Course...</option>' + options;
        if (modalSelect) modalSelect.innerHTML = '<option value="">Select a Course...</option>' + options;
        
        loadModules();
    } catch (err) {
        console.error('Error populating course dropdowns:', err);
    }
}

// Current selected module
let selectedModule = null;

// Load Course Modules
async function loadModules() {
    const courseId = document.getElementById('moduleCourseFilter').value;
    const table = document.getElementById('modulesTable');
    
    if (!courseId) {
        table.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Please select a course to view modules.</td></tr>';
        document.getElementById('lessonsSection').style.display = 'none';
        selectedModule = null;
        return;
    }

    try {
        const modules = await fetch(`${API_URL}/coursemodules/course/${courseId}`).then(r => r.json());
        
        if (modules.length === 0) {
            table.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No modules found for this course.</td></tr>';
            document.getElementById('lessonsSection').style.display = 'none';
            selectedModule = null;
            return;
        }

        table.innerHTML = modules.map(m => `
            <tr style="cursor: pointer;" onclick="selectModule(${JSON.stringify(m).replace(/"/g, '&quot;')})" class="${selectedModule?.id === m.id ? 'table-active' : ''}">
                <td>${m.moduleNumber}</td>
                <td>${m.title}</td>
                <td>${m.durationMinutes} min</td>
                <td>${m.displayOrder}</td>
                <td>${m.isPublished 
                    ? '<span class="badge bg-success">Published</span>' 
                    : '<span class="badge bg-secondary">Draft</span>'}</td>
                <td onclick="event.stopPropagation()">
                    <button class="btn btn-sm btn-warning" onclick="editModule(${m.id})">Edit</button>
                    ${m.isPublished
                        ? `<button class="btn btn-sm btn-outline-secondary" onclick="unpublishModule(${m.id})">Unpublish</button>`
                        : `<button class="btn btn-sm btn-success" onclick="publishModule(${m.id})">Publish</button>`
                    }
                    <button class="btn btn-sm btn-danger" onclick="deleteModule(${m.id})">Delete</button>
                </td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading modules:', err);
    }
}

// Select a module
function selectModule(module) {
    selectedModule = module;
    document.getElementById('selectedModuleTitle').textContent = module.title;
    document.getElementById('lessonsSection').style.display = 'block';
    loadModules();
    loadLessons();
}

// Load Lessons for selected module
async function loadLessons() {
    if (!selectedModule) return;
    const table = document.getElementById('lessonsTable');

    try {
        const lessons = await fetch(`${API_URL}/lessons/module/${selectedModule.id}`).then(r => r.json());
        
        if (lessons.length === 0) {
            table.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No lessons found for this module.</td></tr>';
            return;
        }

        table.innerHTML = lessons.map(l => `
            <tr>
                <td>${l.displayOrder}</td>
                <td>${l.title}</td>
                <td>${Math.floor(l.durationSeconds / 60)}:${(l.durationSeconds % 60).toString().padStart(2, '0')}</td>
                <td>${l.isPreview ? '<span class="badge bg-info">Yes</span>' : 'No'}</td>
                <td>${l.isPublished ? '<span class="badge bg-success">Yes</span>' : 'No'}</td>
                <td>
                    <button class="btn btn-sm btn-warning" onclick="editLesson(${l.id})">Edit</button>
                    <button class="btn btn-sm btn-danger" onclick="deleteLesson(${l.id})">Delete</button>
                </td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading lessons:', err);
    }
}

// Show lesson form
let lessonModal;
function showLessonForm(lesson = null) {
    if (!lessonModal) lessonModal = new bootstrap.Modal(document.getElementById('lessonModal'));
    
    const form = document.getElementById('lessonForm');
    form.reset();
    
    if (lesson) {
        document.getElementById('lessonModalTitle').textContent = 'Edit Lesson';
        document.getElementById('lessonId').value = lesson.id;
        document.getElementById('lessonModuleId').value = lesson.moduleId;
        document.getElementById('lessonTitle').value = lesson.title;
        document.getElementById('lessonDescription').value = lesson.description;
        document.getElementById('lessonDuration').value = lesson.durationSeconds;
        document.getElementById('lessonOrder').value = lesson.displayOrder;
        document.getElementById('lessonVideoUrl').value = lesson.videoUrl || '';
        document.getElementById('lessonIsPreview').checked = lesson.isPreview;
        document.getElementById('lessonPublished').checked = lesson.isPublished;
    } else {
        document.getElementById('lessonModalTitle').textContent = 'Add Lesson';
        document.getElementById('lessonId').value = '';
        document.getElementById('lessonModuleId').value = selectedModule.id;
    }
    
    lessonModal.show();
}

// Save Lesson
async function saveLesson() {
    const id = document.getElementById('lessonId').value;
    const moduleId = document.getElementById('lessonModuleId').value;
    const token = getAdminToken();
    if (!moduleId) return alert('Please select a module first');
    
    const lessonData = {
        moduleId: parseInt(moduleId),
        title: document.getElementById('lessonTitle').value,
        description: document.getElementById('lessonDescription').value,
        durationSeconds: parseInt(document.getElementById('lessonDuration').value),
        displayOrder: parseInt(document.getElementById('lessonOrder').value),
        videoUrl: document.getElementById('lessonVideoUrl').value,
        content: '',
        isPublished: document.getElementById('lessonPublished').checked,
        isPreview: document.getElementById('lessonIsPreview').checked
    };
    
    const url = id ? `${API_URL}/lessons/${id}` : `${API_URL}/lessons`;
    const method = id ? 'PUT' : 'POST';
    
    try {
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(lessonData)
        });
        
        if (response.ok) {
            lessonModal.hide();
            loadLessons();
        } else {
            const err = await response.json();
            alert('Error saving lesson: ' + (err.message || 'Unknown error'));
        }
    } catch (err) {
        console.error('Error saving lesson:', err);
        alert('Error saving lesson');
    }
}

// Edit Lesson
async function editLesson(id) {
    const token = getAdminToken();
    try {
        const lesson = await fetch(`${API_URL}/lessons/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        }).then(r => r.json());
        showLessonForm(lesson);
    } catch (err) {
        console.error('Error fetching lesson:', err);
    }
}

// Delete Lesson
async function deleteLesson(id) {
    const token = getAdminToken();
    if (!confirm('Are you sure you want to delete this lesson?')) return;
    
    try {
        const response = await fetch(`${API_URL}/lessons/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (response.ok) {
            loadLessons();
        } else {
            alert('Error deleting lesson');
        }
    } catch (err) {
        console.error('Error deleting lesson:', err);
    }
}

// Show module add/edit form modal
let moduleModal;
function showModuleForm(module = null) {
    if (!moduleModal) moduleModal = new bootstrap.Modal(document.getElementById('moduleModal'));
    
    const form = document.getElementById('moduleForm');
    form.reset();
    
    if (module) {
        document.getElementById('moduleModalTitle').textContent = 'Edit Module';
        document.getElementById('moduleId').value = module.id;
        document.getElementById('moduleCourseId').value = module.courseId;
        document.getElementById('moduleNumber').value = module.moduleNumber;
        document.getElementById('moduleTitle').value = module.title;
        document.getElementById('moduleDescription').value = module.description;
        document.getElementById('moduleDuration').value = module.durationMinutes;
        document.getElementById('moduleOrder').value = module.displayOrder;
        document.getElementById('modulePublished').checked = module.isPublished;
    } else {
        document.getElementById('moduleModalTitle').textContent = 'Add Module';
        document.getElementById('moduleId').value = '';
        // Default new modules to Published so they show on the landing page immediately
        document.getElementById('modulePublished').checked = true;
        const filterVal = document.getElementById('moduleCourseFilter').value;
        if (filterVal) document.getElementById('moduleCourseId').value = filterVal;
    }
    
    moduleModal.show();
}

// Save Module
async function saveModule() {
    const id = document.getElementById('moduleId').value;
    const courseIdVal = document.getElementById('moduleCourseId').value;
    const token = getAdminToken();
    if (!courseIdVal) return alert('Please select a course');

    const moduleData = {
        courseId: parseInt(courseIdVal),
        moduleNumber: parseInt(document.getElementById('moduleNumber').value),
        title: document.getElementById('moduleTitle').value,
        description: document.getElementById('moduleDescription').value,
        durationMinutes: parseInt(document.getElementById('moduleDuration').value),
        displayOrder: parseInt(document.getElementById('moduleOrder').value),
        isPublished: document.getElementById('modulePublished').checked
    };

    const url = id ? `${API_URL}/coursemodules/${id}` : `${API_URL}/coursemodules`;
    const method = id ? 'PUT' : 'POST';

    try {
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(moduleData)
        });

        if (response.ok) {
            moduleModal.hide();
            loadModules();
        } else {
            const err = await response.json();
            alert('Error saving module: ' + (err.message || 'Unknown error'));
        }
    } catch (err) {
        console.error('Error saving module:', err);
    }
}

// Edit Module load details
async function editModule(id) {
    try {
        const module = await fetch(`${API_URL}/coursemodules/${id}`).then(r => r.json());
        showModuleForm(module);
    } catch (err) {
        console.error('Error fetching module details:', err);
    }
}

// Delete Module
async function deleteModule(id) {
    const token = getAdminToken();
    if (!confirm('Are you sure you want to delete this module?')) return;

    try {
        const response = await fetch(`${API_URL}/coursemodules/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            loadModules();
        } else {
            alert('Error deleting module');
        }
    } catch (err) {
        console.error('Error deleting module:', err);
    }
}

// Publish Module (quick toggle)
async function publishModule(id) {
    const token = getAdminToken();
    try {
        const response = await fetch(`${API_URL}/coursemodules/${id}/publish`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
            loadModules();
        } else {
            alert('Error publishing module');
        }
    } catch (err) {
        console.error('Error publishing module:', err);
    }
}

// Unpublish Module (quick toggle)
async function unpublishModule(id) {
    const token = getAdminToken();
    try {
        const response = await fetch(`${API_URL}/coursemodules/${id}/unpublish`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
            loadModules();
        } else {
            alert('Error unpublishing module');
        }
    } catch (err) {
        console.error('Error unpublishing module:', err);
    }
}


async function loadTestimonials() {
    const token = getAdminToken();
    try {
        const testimonials = await fetch(API_URL + '/testimonials', {
            headers: { 'Authorization': `Bearer ${token}` }
        }).then(r => r.json());

        const table = document.getElementById('testimonialsTable');
        table.innerHTML = testimonials.map(t => `
            <tr>
                <td>${t.studentName}</td>
                <td>${t.studentRole}</td>
                <td>
                    <span class="badge bg-warning">
                        <i class="fas fa-star"></i> ${t.rating}/5
                    </span>
                </td>
                <td>${t.isPublished ? '<span class="badge bg-success">Yes</span>' : '<span class="badge bg-secondary">No</span>'}</td>
                <td>
                    <button class="btn btn-sm btn-warning" onclick="editTestimonial(${t.id})">Edit</button>
                    <button class="btn btn-sm btn-danger" onclick="deleteTestimonial(${t.id})">Delete</button>
                </td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading testimonials:', err);
    }
}

// Show testimonial modal form
let testimonialModal;
function showTestimonialForm(testimonial = null) {
    if (!testimonialModal) testimonialModal = new bootstrap.Modal(document.getElementById('testimonialModal'));
    
    const form = document.getElementById('testimonialForm');
    form.reset();

    // Handle file upload
    document.getElementById('testimonialImageFile').onchange = async (e) => {
        const file = e.target.files[0];
        if (!file) return;
        const formData = new FormData();
        formData.append('file', file);
        const token = getAdminToken();
        const response = await fetch(`${API_URL}/files/upload-image`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` },
            body: formData
        }).then(r => r.json());
        if (response.success) document.getElementById('testimonialImageUrl').value = response.filePath;
    };
    
    if (testimonial) {
        document.getElementById('testimonialModalTitle').textContent = 'Edit Testimonial';
        document.getElementById('testimonialId').value = testimonial.id;
        document.getElementById('testimonialName').value = testimonial.studentName;
        document.getElementById('testimonialRole').value = testimonial.studentRole;
        document.getElementById('testimonialImageUrl').value = testimonial.studentImageUrl || '';
        document.getElementById('testimonialRating').value = testimonial.rating;
        document.getElementById('testimonialContent').value = testimonial.content;
        document.getElementById('testimonialPublished').checked = testimonial.isPublished;
    } else {
        document.getElementById('testimonialModalTitle').textContent = 'Add Testimonial';
        document.getElementById('testimonialId').value = '';
    }
    
    testimonialModal.show();
}

// Save Testimonial
async function saveTestimonial() {
    const id = document.getElementById('testimonialId').value;
    const token = getAdminToken();
    const data = {
        studentName: document.getElementById('testimonialName').value,
        studentRole: document.getElementById('testimonialRole').value,
        studentImageUrl: document.getElementById('testimonialImageUrl').value,
        rating: parseInt(document.getElementById('testimonialRating').value),
        content: document.getElementById('testimonialContent').value,
        isPublished: document.getElementById('testimonialPublished').checked,
        displayOrder: 0
    };

    const method = id ? 'PUT' : 'POST';
    const url = id ? `${API_URL}/testimonials/${id}` : `${API_URL}/testimonials`;

    try {
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            testimonialModal.hide();
            loadTestimonials();
        } else {
            alert('Error saving testimonial');
        }
    } catch (err) {
        console.error('Error saving testimonial:', err);
    }
}

// Edit Testimonial details
async function editTestimonial(id) {
    const token = getAdminToken();
    try {
        const testimonial = await fetch(`${API_URL}/testimonials/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        }).then(r => r.json());
        showTestimonialForm(testimonial);
    } catch (err) {
        console.error('Error fetching testimonial:', err);
    }
}

// Delete Testimonial
async function deleteTestimonial(id) {
    const token = getAdminToken();
    if (!confirm('Are you sure?')) return;
    try {
        const response = await fetch(`${API_URL}/testimonials/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
            loadTestimonials();
        }
    } catch (err) {
        console.error('Error deleting testimonial:', err);
    }
}

// Load Pricing Tiers
async function loadPricing() {
    const token = getAdminToken();
    try {
        const tiers = await fetch(API_URL + '/pricing', {
            headers: { 'Authorization': `Bearer ${token}` }
        }).then(r => r.json());

        const table = document.getElementById('pricingTable');
        table.innerHTML = tiers.map(t => `
            <tr>
                <td>${t.name}</td>
                <td>$${t.price}</td>
                <td>${t.billingPeriod}</td>
                <td>${t.isPopular ? '<span class="badge bg-warning">Yes</span>' : 'No'}</td>
                <td>${t.isPublished ? '<span class="badge bg-success">Yes</span>' : '<span class="badge bg-secondary">No</span>'}</td>
                <td>
                    <button class="btn btn-sm btn-warning" onclick="editPricing(${t.id})">Edit</button>
                    <button class="btn btn-sm btn-danger" onclick="deletePricing(${t.id})">Delete</button>
                </td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading pricing:', err);
    }
}

// Show pricing modal form
let pricingModal;
function showPricingForm(tier = null) {
    if (!pricingModal) pricingModal = new bootstrap.Modal(document.getElementById('pricingModal'));
    
    const form = document.getElementById('pricingForm');
    form.reset();
    
    if (tier) {
        document.getElementById('pricingModalTitle').textContent = 'Edit Pricing Tier';
        document.getElementById('pricingId').value = tier.id;
        document.getElementById('pricingName').value = tier.name;
        document.getElementById('pricingPrice').value = tier.price;
        document.getElementById('pricingPeriod').value = tier.billingPeriod;
        document.getElementById('pricingDescription').value = tier.description;
        document.getElementById('pricingPopular').checked = tier.isPopular;
        document.getElementById('pricingPublished').checked = tier.isPublished;
        
        const features = tier.features.map(f => f.feature).join('\n');
        document.getElementById('pricingFeatures').value = features;
    } else {
        document.getElementById('pricingModalTitle').textContent = 'Add Pricing Tier';
        document.getElementById('pricingId').value = '';
    }
    
    pricingModal.show();
}

// Save Pricing Tier
async function savePricing() {
    const id = document.getElementById('pricingId').value;
    const token = getAdminToken();
    const pricingData = {
        name: document.getElementById('pricingName').value,
        price: parseFloat(document.getElementById('pricingPrice').value),
        billingPeriod: document.getElementById('pricingPeriod').value,
        description: document.getElementById('pricingDescription').value,
        isPopular: document.getElementById('pricingPopular').checked,
        isPublished: document.getElementById('pricingPublished').checked,
        features: document.getElementById('pricingFeatures').value.split('\n').filter(f => f.trim() !== '')
    };

    const method = id ? 'PUT' : 'POST';
    const url = id ? `${API_URL}/pricing/${id}` : `${API_URL}/pricing`;

    try {
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(pricingData)
        });

        if (response.ok) {
            pricingModal.hide();
            loadPricing();
        } else {
            const err = await response.json();
            alert('Error saving pricing: ' + (err.message || 'Unknown error'));
        }
    } catch (err) {
        console.error('Error saving pricing:', err);
        alert('Error saving pricing');
    }
}

// Edit Pricing Tier details load
async function editPricing(id) {
    const token = getAdminToken();
    try {
        const tier = await fetch(`${API_URL}/pricing/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        }).then(r => r.json());
        showPricingForm(tier);
    } catch (err) {
        console.error('Error fetching pricing:', err);
    }
}

// Delete Pricing Tier
async function deletePricing(id) {
    const token = getAdminToken();
    if (!confirm('Are you sure you want to delete this pricing tier?')) return;

    try {
        const response = await fetch(`${API_URL}/pricing/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            loadPricing();
        } else {
            alert('Error deleting pricing tier');
        }
    } catch (err) {
        console.error('Error deleting pricing tier:', err);
    }
}

// File Upload Operations
async function uploadImage() {
    const file = document.getElementById('imageFile').files[0];
    const token = getAdminToken();
    if (!file) return alert('Please select a file');

    const formData = new FormData();
    formData.append('file', file);

    try {
        const response = await fetch(API_URL + '/files/upload-image', {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` },
            body: formData
        }).then(r => r.json());

        const div = document.getElementById('imageResponse');
        if (response.success) {
            div.innerHTML = `<div class="alert alert-success">File uploaded: ${response.filePath}</div>`;
        } else {
            div.innerHTML = `<div class="alert alert-danger">${response.message}</div>`;
        }
    } catch (err) {
        console.error('Error uploading:', err);
    }
}

async function uploadVideo() {
    const file = document.getElementById('videoFile').files[0];
    const token = getAdminToken();
    if (!file) return alert('Please select a file');

    const formData = new FormData();
    formData.append('file', file);

    try {
        const response = await fetch(API_URL + '/files/upload-video', {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` },
            body: formData
        }).then(r => r.json());

        const div = document.getElementById('videoResponse');
        if (response.success) {
            div.innerHTML = `<div class="alert alert-success">File uploaded: ${response.filePath}</div>`;
        } else {
            div.innerHTML = `<div class="alert alert-danger">${response.message}</div>`;
        }
    } catch (err) {
        console.error('Error uploading:', err);
    }
}

// Global hookups to window object for inline event handler visibility
window.handleLogin = handleLogin;
window.logout = logout;
window.loadDashboard = loadDashboard;
window.loadHero = loadHero;
window.saveHero = saveHero;
window.loadCourses = loadCourses;
window.showCourseForm = showCourseForm;
window.saveCourse = saveCourse;
window.editCourse = editCourse;
window.deleteCourse = deleteCourse;
window.populateCourseDropdowns = populateCourseDropdowns;
window.loadModules = loadModules;
window.showModuleForm = showModuleForm;
window.saveModule = saveModule;
window.editModule = editModule;
window.deleteModule = deleteModule;
window.publishModule = publishModule;
window.unpublishModule = unpublishModule;
window.loadTestimonials = loadTestimonials;
window.showTestimonialForm = showTestimonialForm;
window.saveTestimonial = saveTestimonial;
window.editTestimonial = editTestimonial;
window.deleteTestimonial = deleteTestimonial;
window.loadPricing = loadPricing;
window.showPricingForm = showPricingForm;
window.savePricing = savePricing;
window.editPricing = editPricing;
window.deletePricing = deletePricing;
window.uploadImage = uploadImage;
window.uploadVideo = uploadVideo;
window.getAdminToken = getAdminToken;
window.checkAdminAuth = checkAdminAuth;
