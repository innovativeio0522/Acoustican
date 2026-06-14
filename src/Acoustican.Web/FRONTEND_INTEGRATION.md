# Frontend Integration Guide

This guide shows how to integrate the Acoustican frontend website with the admin API to display dynamically managed content.

## Overview

The admin API provides public endpoints that don't require authentication for published content:

- **Testimonials**: `GET /api/testimonials/published`
- **Pricing**: `GET /api/pricing/published`
- **Courses** (with restrictions): `GET /api/courses` (requires auth to show all)

Admin-only content requires JWT authentication.

## Setup in Frontend

### 1. Configure API Base URL

In your frontend project (where the main website is), add configuration:

```javascript
// config.js or similar
const API_CONFIG = {
  baseURL: 'https://localhost:7001/api', // Change for production
  timeout: 30000
};

// Create axios instance or fetch wrapper
const apiClient = {
  get: async (endpoint) => {
    const response = await fetch(`${API_CONFIG.baseURL}${endpoint}`);
    if (!response.ok) throw new Error(`API Error: ${response.status}`);
    return response.json();
  }
};
```

### 2. Load Published Testimonials

Replace the static testimonials in your website with dynamic data:

```javascript
// In your main script.js or testimonials component
async function loadTestimonials() {
  try {
    const testimonials = await fetch('https://localhost:7001/api/testimonials/published')
      .then(r => r.json());
    
    // Render testimonials
    const container = document.querySelector('.testimonials-container');
    container.innerHTML = testimonials.map(t => `
      <div class="testimonial-card">
        ${t.studentImageUrl ? `<img src="${t.studentImageUrl}" alt="${t.studentName}">` : ''}
        <div class="stars">${'★'.repeat(t.rating)}${'☆'.repeat(5-t.rating)}</div>
        <p class="testimonial-text">"${t.content}"</p>
        <div class="student-info">
          <strong>${t.studentName}</strong>
          <small>${t.studentRole}</small>
        </div>
      </div>
    `).join('');
  } catch (err) {
    console.error('Error loading testimonials:', err);
  }
}

// Call on page load
document.addEventListener('DOMContentLoaded', loadTestimonials);
```

### 3. Load Published Pricing Tiers

Update the pricing section dynamically:

```javascript
async function loadPricingTiers() {
  try {
    const tiers = await fetch('https://localhost:7001/api/pricing/published')
      .then(r => r.json());
    
    // Sort by display order
    tiers.sort((a, b) => a.displayOrder - b.displayOrder);
    
    // Render pricing cards
    const container = document.querySelector('.pricing-container');
    container.innerHTML = tiers.map(tier => `
      <div class="pricing-card ${tier.isPopular ? 'popular' : ''}">
        ${tier.isPopular ? '<div class="popular-badge">Most Popular</div>' : ''}
        <h3>${tier.name}</h3>
        <div class="price">
          <span class="amount">$${tier.price}</span>
          <span class="period">/${tier.billingPeriod}</span>
        </div>
        <p class="description">${tier.description}</p>
        <ul class="features">
          ${tier.features.map(f => `
            <li>
              <i class="fas fa-check"></i>
              ${f.feature}
            </li>
          `).join('')}
        </ul>
        <button class="btn-purchase">Get Started</button>
      </div>
    `).join('');
  } catch (err) {
    console.error('Error loading pricing:', err);
  }
}

document.addEventListener('DOMContentLoaded', loadPricingTiers);
```

### 4. Load Courses (With Authentication)

For admin to see all courses:

```javascript
async function loadCoursesForAdmin(token) {
  try {
    const courses = await fetch('https://localhost:7001/api/courses', {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    }).then(r => r.json());
    
    // Render courses
    renderCourses(courses);
  } catch (err) {
    console.error('Error loading courses:', err);
  }
}

// For public - only published courses
async function loadPublishedCourses() {
  try {
    const courses = await fetch('https://localhost:7001/api/courses')
      .then(r => r.json());
    
    const published = courses.filter(c => c.isPublished);
    renderCourses(published);
  } catch (err) {
    console.error('Error loading courses:', err);
  }
}
```

## Real-Time Content Updates

### Auto-Refresh Every 5 Minutes

```javascript
// Auto-refresh content periodically
function startContentRefresh() {
  // Initial load
  loadTestimonials();
  loadPricingTiers();
  
  // Refresh every 5 minutes
  setInterval(() => {
    loadTestimonials();
    loadPricingTiers();
  }, 5 * 60 * 1000);
}

document.addEventListener('DOMContentLoaded', startContentRefresh);
```

### Handle API Errors Gracefully

```javascript
async function loadWithFallback(endpoint, fallbackData) {
  try {
    const response = await fetch(endpoint);
    if (!response.ok) throw new Error('API Error');
    return await response.json();
  } catch (err) {
    console.error(`Error loading from ${endpoint}:`, err);
    // Use fallback/cached data
    return fallbackData || [];
  }
}
```

## Update HTML Structure

Replace static content in [index.html](../index.html):

### Testimonials Section

```html
<!-- Before: Static testimonials -->
<section class="testimonials">
    <h2>What Our Students Say</h2>
    <div class="testimonials-container">
        <!-- Static cards here -->
    </div>
</section>

<!-- After: Dynamic testimonials -->
<section class="testimonials">
    <h2>What Our Students Say</h2>
    <div class="testimonials-container" id="testimonials">
        <div class="loading">Loading testimonials...</div>
    </div>
</section>
```

### Pricing Section

```html
<!-- Before -->
<section id="pricing" class="pricing-section">
    <div class="pricing-grid">
        <!-- Static pricing cards -->
    </div>
</section>

<!-- After -->
<section id="pricing" class="pricing-section">
    <div class="pricing-container" id="pricing-container">
        <div class="loading">Loading pricing...</div>
    </div>
</section>
```

## Admin Features

### Create Course from Frontend

```javascript
async function createCourse(courseData) {
  const token = localStorage.getItem('adminToken');
  
  try {
    const response = await fetch('https://localhost:7001/api/courses', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(courseData)
    }).then(r => r.json());
    
    if (response.id) {
      console.log('Course created:', response);
      // Refresh course list
      loadCoursesForAdmin(token);
    }
  } catch (err) {
    console.error('Error creating course:', err);
  }
}

// Usage
createCourse({
  title: 'Advanced Fingerpicking',
  description: 'Master advanced fingerpicking techniques',
  level: 'Advanced',
  price: 49.99,
  durationMinutes: 480,
  isPublished: false
});
```

### Upload Course Thumbnail

```javascript
async function uploadCourseThumbnail(courseId, file) {
  const token = localStorage.getItem('adminToken');
  const formData = new FormData();
  formData.append('file', file);
  
  try {
    const response = await fetch(
      `https://localhost:7001/api/files/upload-image`,
      {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
        body: formData
      }
    ).then(r => r.json());
    
    if (response.success) {
      // Update course with thumbnail URL
      console.log('Thumbnail uploaded:', response.filePath);
    }
  } catch (err) {
    console.error('Error uploading thumbnail:', err);
  }
}

// Usage
const fileInput = document.getElementById('thumbnail');
uploadCourseThumbnail(1, fileInput.files[0]);
```

## Environment Variables

Create `.env` or `config.json` for different environments:

```javascript
// config.development.js
export const API_BASE_URL = 'https://localhost:7001/api';

// config.production.js
export const API_BASE_URL = 'https://api.acoustican.com/api';

// Usage in app
import { API_BASE_URL } from './config';

async function loadTestimonials() {
  const response = await fetch(`${API_BASE_URL}/testimonials/published`);
  // ...
}
```

## Caching Strategy

### Browser Cache

```javascript
// Add cache headers
const cacheConfig = {
  testimonials: { ttl: 10 * 60 * 1000 }, // 10 minutes
  pricing: { ttl: 10 * 60 * 1000 },      // 10 minutes
  courses: { ttl: 5 * 60 * 1000 }        // 5 minutes
};

const cache = {};

async function getCachedData(key, fetcher) {
  const now = Date.now();
  
  if (cache[key] && (now - cache[key].timestamp) < cacheConfig[key].ttl) {
    return cache[key].data;
  }
  
  const data = await fetcher();
  cache[key] = { data, timestamp: now };
  return data;
}

// Usage
const testimonials = await getCachedData('testimonials', 
  () => fetch(`${API_BASE_URL}/testimonials/published`).then(r => r.json())
);
```

### LocalStorage Cache

```javascript
function saveCacheToStorage(key, data) {
  const cacheData = {
    data: data,
    timestamp: Date.now()
  };
  localStorage.setItem(`cache_${key}`, JSON.stringify(cacheData));
}

function loadCacheFromStorage(key, maxAge = 30 * 60 * 1000) {
  const cached = localStorage.getItem(`cache_${key}`);
  if (!cached) return null;
  
  const { data, timestamp } = JSON.parse(cached);
  if (Date.now() - timestamp > maxAge) {
    localStorage.removeItem(`cache_${key}`);
    return null;
  }
  
  return data;
}

// Usage
async function loadTestimonialsWithCache() {
  // Try cache first
  let testimonials = loadCacheFromStorage('testimonials');
  
  if (!testimonials) {
    // Fetch fresh data
    testimonials = await fetch(`${API_BASE_URL}/testimonials/published`)
      .then(r => r.json());
    
    // Save to cache
    saveCacheToStorage('testimonials', testimonials);
  }
  
  return testimonials;
}
```

## Error Handling

```javascript
async function fetchWithErrorHandling(url, options = {}) {
  try {
    const response = await fetch(url, options);
    
    if (response.status === 401) {
      // Unauthorized - redirect to login
      window.location.href = '/admin/login';
      return null;
    }
    
    if (response.status === 403) {
      console.error('Forbidden: You do not have permission');
      return null;
    }
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }
    
    return await response.json();
  } catch (err) {
    console.error('Fetch error:', err);
    // Show user-friendly error message
    showNotification('Error loading data', 'error');
    return null;
  }
}
```

## Loading States

```html
<div class="testimonials-container">
  <div class="loading-skeleton">
    <div class="skeleton-card"></div>
    <div class="skeleton-card"></div>
    <div class="skeleton-card"></div>
  </div>
</div>

<style>
.loading-skeleton {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
}

.skeleton-card {
  background: linear-gradient(90deg, #2a2a2a 25%, #3a3a3a 50%, #2a2a2a 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
  height: 250px;
  border-radius: 8px;
}

@keyframes loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
</style>
```

## CORS Issues

If you get CORS errors, ensure:

1. **Backend** has CORS enabled in `Program.cs`:
```csharp
app.UseCors("AllowAll"); // or configure specific origins
```

2. **Frontend** requests use correct headers:
```javascript
fetch(url, {
  headers: {
    'Content-Type': 'application/json'
    // Other headers...
  }
});
```

3. **HTTPS** URLs match (http/https must be consistent)

## Testing API Endpoints

Use curl or Postman to test endpoints:

```bash
# Get published testimonials
curl https://localhost:7001/api/testimonials/published

# Get published pricing (no auth required)
curl https://localhost:7001/api/pricing/published

# Get all courses (requires auth)
curl -H "Authorization: Bearer YOUR_TOKEN" https://localhost:7001/api/courses
```

## Next Steps

1. Update your HTML to remove static content
2. Implement the fetch functions above
3. Test with the admin panel adding/editing content
4. Deploy to production with proper HTTPS and domain
5. Update API_BASE_URL for production

## Support

For issues or questions, refer to:
- [Admin API README](./README.md)
- [Setup Guide](./SETUP.md)
- [API Endpoints](./README.md#api-endpoints)
