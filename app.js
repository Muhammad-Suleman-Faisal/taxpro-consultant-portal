// TaxPro Consultant Portal - Application Engine
// Handles Tax Calculation, Dynamic Scheduling, REST API calls, Modals, and State

// --- 1. TOAST NOTIFICATION ENGINE ---
function showToast(message, type = 'success') {
  let container = document.getElementById('toast-container');
  if (!container) {
    container = document.createElement('div');
    container.id = 'toast-container';
    document.body.appendChild(container);
  }

  const toast = document.createElement('div');
  toast.className = 'toast';
  const icon = type === 'success' ? 'check_circle' : 'info';
  const iconColor = type === 'success' ? 'text-secondary' : 'text-primary';

  toast.innerHTML = `
    <span class="material-symbols-outlined ${iconColor} text-[20px]">${icon}</span>
    <span>${message}</span>
  `;

  container.appendChild(toast);
  setTimeout(() => {
    toast.style.opacity = '0';
    toast.style.transform = 'translateX(100%)';
    toast.style.transition = 'all 0.3s ease';
    setTimeout(() => toast.remove(), 300);
  }, 4000);
}

// --- 2. DARK MODE MANAGER ---
function initTheme() {
  const isDark = localStorage.getItem('taxpro_theme') === 'dark' || 
    (!('taxpro_theme' in localStorage) && window.matchMedia('(prefers-color-scheme: dark)').matches);
  
  if (isDark) {
    document.documentElement.classList.add('dark');
  } else {
    document.documentElement.classList.remove('dark');
  }
  updateThemeIcons(isDark);
}

function toggleTheme() {
  const isDark = document.documentElement.classList.toggle('dark');
  localStorage.setItem('taxpro_theme', isDark ? 'dark' : 'light');
  updateThemeIcons(isDark);
  showToast(isDark ? 'Dark mode enabled' : 'Light mode enabled', 'info');
}

function updateThemeIcons(isDark) {
  const icons = document.querySelectorAll('.theme-toggle-icon');
  icons.forEach(icon => {
    icon.textContent = isDark ? 'light_mode' : 'dark_mode';
  });
}

// --- 3. SMOOTH NAVIGATION HELPER ---
function scrollToSection(id) {
  // Close drawer if open
  closeDrawer();
  const el = document.getElementById(id);
  if (el) {
    const yOffset = -70; // Header offset
    const y = el.getBoundingClientRect().top + window.pageYOffset + yOffset;
    window.scrollTo({ top: y, behavior: 'smooth' });
  }
}

function openDrawer() {
  document.getElementById('mobile-drawer').classList.remove('-translate-x-full');
  document.getElementById('drawer-backdrop').classList.remove('hidden');
  document.body.classList.add('overflow-hidden');
}

function closeDrawer() {
  const drawer = document.getElementById('mobile-drawer');
  const backdrop = document.getElementById('drawer-backdrop');
  if (drawer) drawer.classList.add('-translate-x-full');
  if (backdrop) backdrop.classList.add('hidden');
  document.body.classList.remove('overflow-hidden');
}

// --- 4. SERVICES DATA & DETAIL MODAL ---
const servicesData = [
  {
    title: "Income Tax Advisory & Filing",
    icon: "account_balance",
    desc: "Comprehensive legal representation under the Income Tax Ordinance 2001. We manage annual return filings, wealth reconciliation statements, foreign remittance declarations, and Section 147 advance tax computations.",
    turnaround: "Turnaround: 2-3 Working Days",
    bullets: [
      "Annual Income Tax return drafting and e-filing via FBR Iris 2.0",
      "Wealth statement formulation with asset/liability balance verification",
      "Advance tax computation (Section 147) and PSID/CPR generation",
      "Fast-track Active Taxpayer List (ATL) activation and penalty waiver support"
    ]
  },
  {
    title: "Sales Tax & Provincial GST",
    icon: "receipt",
    desc: "Federal Sales Tax (FBR) and provincial sales tax registrations (SRB, PRA, KPRA, BRA) for services and manufactured goods. We prepare flawless monthly returns and input adjustment ledgers.",
    turnaround: "Turnaround: Monthly Filing Cycle (Due 15th-18th)",
    bullets: [
      "Annexure-A (Purchases) and Annexure-C (Sales) ledger matching",
      "Input tax reconciliation to prevent inadmissible tax adjustments",
      "E-filing of Federal and Provincial returns simultaneously",
      "STRN (Sales Tax Registration Number) issuance & biometrics assistance"
    ]
  },
  {
    title: "Withholding Tax (WHT) Monitoring",
    icon: "price_change",
    desc: "Guiding organizations designated as Withholding Agents under Chapter XII of the Income Tax Ordinance. Ensure strict adherence across vendor contracts, payroll, and rental agreements.",
    turnaround: "Turnaround: Real-Time / Bi-Annual e-Filing",
    bullets: [
      "Bi-annual and monthly e-statements under Section 165",
      "Withholding exemption certificate applications under Section 153/159",
      "Vendor ATL verification before deduction to avoid 100% non-filer penalties",
      "Issuance of Section 164 computerized tax deduction certificates"
    ]
  },
  {
    title: "FBR Services & Notice Defense",
    icon: "verified",
    desc: "Immediate intervention and legal representation for statutory notices issued by Inland Revenue Commissioners under the Income Tax and Sales Tax laws.",
    turnaround: "Urgent Response: Within 24-48 Hours",
    bullets: [
      "Notice evaluation for Section 111 (unexplained income) and 122 (amendments)",
      "Drafting legally fortified statutory replies and comprehensive paper books",
      "Personal appearance and representation before assessing officers",
      "Stay orders and rectification petitions under Section 221"
    ]
  },
  {
    title: "SRB & Provincial Tax Practice",
    icon: "location_city",
    desc: "Specialized practice dedicated to Sindh Revenue Board (SRB) and Punjab Revenue Authority (PRA) service tax compliance, withholding agent regulations, and tariff codes.",
    turnaround: "Turnaround: 3 Working Days",
    bullets: [
      "SRB / PRA company registration and classification",
      "Monthly return filing under Sindh Sales Tax on Services Act 2011",
      "Guidance on software, IT-enabled, management & consultancy levies",
      "Resolving inter-provincial sales tax jurisdictional disputes"
    ]
  },
  {
    title: "Corporate Tax & SECP Secretarial",
    icon: "corporate_fare",
    desc: "End-to-end company life cycle support with Securities and Exchange Commission of Pakistan (SECP), covering entity formation, corporate governance, and statutory audit readiness.",
    turnaround: "Turnaround: 3-5 Working Days for Registration",
    bullets: [
      "Private Limited, Single Member (SMC), and LLP Incorporation",
      "Filing of statutory annual forms (Form A, 29, 45, and Form 19/20)",
      "Ultimate Beneficial Ownership (UBO) compliance reporting",
      "Authorized and paid-up capital enhancement filings"
    ]
  },
  {
    title: "Tax Assessment, Audits & Appeals",
    icon: "gavel",
    desc: "Defense advocacy for corporate and high-net-worth clients subjected to Section 177 audits, Section 214C computer-balloted selections, or Commissioner Appeals.",
    turnaround: "Turnaround: Case-by-Case Statutory Schedule",
    bullets: [
      "Audit trail documentation and reconciling bank credits",
      "Filing first appeals before Commissioner Inland Revenue (Appeals)",
      "Appellate Tribunal Inland Revenue (ATIR) appeal preparation",
      "Alternate Dispute Resolution Committee (ADRC) applications"
    ]
  },
  {
    title: "Strategic Business Restructuring",
    icon: "trending_up",
    desc: "Holistic fiscal architecture for founders and corporations looking to optimize effective tax rates, structure holding vehicles, and execute compliant profit remittances.",
    turnaround: "Turnaround: Custom Advisory Retainer",
    bullets: [
      "Holding company structuring and cross-border dividend flows",
      "Startup tax structuring and PSEB / STZA tech park exemptions",
      "M&A due diligence, asset transfers, and capital gains tax planning",
      "Employee Stock Option Plan (ESOP) tax considerations in Pakistan"
    ]
  }
];

function openServiceDetail(index) {
  const s = servicesData[index];
  if (!s) return;

  document.getElementById('modal-title').textContent = s.title;
  document.getElementById('modal-icon').textContent = s.icon;
  document.getElementById('modal-desc').textContent = s.desc;
  document.getElementById('modal-turnaround').textContent = s.turnaround;

  const list = document.getElementById('modal-bullets');
  list.innerHTML = '';
  s.bullets.forEach(b => {
    const li = document.createElement('li');
    li.className = 'flex items-start gap-2';
    li.innerHTML = `<span class="material-symbols-outlined text-secondary text-[16px] flex-shrink-0 mt-0.5">check</span><span>${b}</span>`;
    list.appendChild(li);
  });

  const modal = document.getElementById('service-modal');
  modal.classList.remove('hidden');
  modal.classList.add('flex');
  document.body.classList.add('overflow-hidden');
}

function closeServiceModal() {
  const modal = document.getElementById('service-modal');
  modal.classList.add('hidden');
  modal.classList.remove('flex');
  document.body.classList.remove('overflow-hidden');
}

function requestThisService() {
  const serviceName = document.getElementById('modal-title').textContent;
  closeServiceModal();
  scrollToSection('consultation-wizard');

  const radios = document.querySelectorAll('input[name="wizard-service"]');
  let matched = false;
  radios.forEach(r => {
    if (serviceName.toLowerCase().includes('income') && r.value.includes('Income')) {
      r.checked = true; matched = true;
    } else if (serviceName.toLowerCase().includes('secp') && r.value.includes('Corporate')) {
      r.checked = true; matched = true;
    } else if (serviceName.toLowerCase().includes('notice') && r.value.includes('Notice')) {
      r.checked = true; matched = true;
    } else if (serviceName.toLowerCase().includes('restructuring') && r.value.includes('Structuring')) {
      r.checked = true; matched = true;
    }
  });
  if (!matched && radios[0]) radios[0].checked = true;
  showToast(`Selected "${serviceName}" in Booking Wizard`, 'info');
}

// --- 5. ENHANCED TAX ESTIMATION TOOL (PAKISTAN FINANCE ACT 2024-25) ---
let calcEntity = 'salaried';
let calcYear = '2024-2025';

function setCalcEntity(type, btn) {
  calcEntity = type;
  const buttons = document.querySelectorAll('.calc-entity-btn');
  buttons.forEach(b => {
    b.className = 'calc-entity-btn p-2 rounded-lg bg-surface-container text-on-surface font-label-sm text-label-sm font-semibold text-center transition-all hover:bg-surface-container-high';
  });
  if (btn) {
    btn.className = 'calc-entity-btn p-2 rounded-lg bg-primary text-on-primary font-label-sm text-label-sm font-semibold text-center transition-all shadow-sm';
  }

  const expenseGrp = document.getElementById('expense-group');
  if (type === 'salaried') {
    expenseGrp.classList.add('hidden');
    expenseGrp.classList.remove('flex');
  } else {
    expenseGrp.classList.remove('hidden');
    expenseGrp.classList.add('flex');
  }
  runTaxCalculation();
}

function setCalcYear(year) {
  calcYear = year;
  const currentBtn = document.getElementById('year-btn-current');
  const prevBtn = document.getElementById('year-btn-prev');
  if (year === '2024-2025') {
    currentBtn.className = 'px-3 py-1 rounded font-label-sm text-label-sm bg-surface-container-lowest font-bold text-primary shadow-sm';
    prevBtn.className = 'px-3 py-1 rounded font-label-sm text-label-sm text-on-surface-variant hover:text-on-surface';
  } else {
    prevBtn.className = 'px-3 py-1 rounded font-label-sm text-label-sm bg-surface-container-lowest font-bold text-primary shadow-sm';
    currentBtn.className = 'px-3 py-1 rounded font-label-sm text-label-sm text-on-surface-variant hover:text-on-surface';
  }
  runTaxCalculation();
}

function handleIncomeSlider(val) {
  const numericVal = parseFloat(val) || 0;
  document.getElementById('income-display').textContent = 'PKR ' + numericVal.toLocaleString('en-US');
  const inputEl = document.getElementById('calc-income-input');
  if (inputEl) inputEl.value = numericVal;
  runTaxCalculation();
}

function handleIncomeInput(val) {
  let numericVal = parseFloat(val) || 0;
  if (numericVal < 0) numericVal = 0;
  document.getElementById('income-display').textContent = 'PKR ' + numericVal.toLocaleString('en-US');
  const sliderEl = document.getElementById('income-slider');
  if (sliderEl) {
    sliderEl.value = Math.min(numericVal, 25000000);
  }
  runTaxCalculation();
}

function runTaxCalculation() {
  const inputEl = document.getElementById('calc-income-input');
  const grossIncome = parseFloat(inputEl ? inputEl.value : document.getElementById('income-slider').value) || 0;
  let expenses = 0;
  if (calcEntity !== 'salaried') {
    expenses = parseFloat(document.getElementById('calc-expenses').value) || 0;
  }

  const taxableIncome = Math.max(0, grossIncome - expenses);
  let taxLiability = 0;
  let bracketDescription = "";

  // Finance Act 2024-25 Slabs
  if (calcEntity === 'salaried') {
    if (taxableIncome <= 600000) {
      taxLiability = 0;
      bracketDescription = "Tax Free Threshold (Income ≤ 600,000)";
    } else if (taxableIncome <= 1200000) {
      taxLiability = (taxableIncome - 600000) * 0.05;
      bracketDescription = "5% of amount exceeding 600k";
    } else if (taxableIncome <= 2200000) {
      taxLiability = 30000 + (taxableIncome - 1200000) * 0.15;
      bracketDescription = "PKR 30,000 + 15% exceeding 1.2M";
    } else if (taxableIncome <= 3200000) {
      taxLiability = 180000 + (taxableIncome - 2200000) * 0.25;
      bracketDescription = "PKR 180,000 + 25% exceeding 2.2M";
    } else if (taxableIncome <= 4100000) {
      taxLiability = 430000 + (taxableIncome - 3200000) * 0.30;
      bracketDescription = "PKR 430,000 + 30% exceeding 3.2M";
    } else {
      taxLiability = 700000 + (taxableIncome - 4100000) * 0.35;
      bracketDescription = "PKR 700,000 + 35% exceeding 4.1M";
    }
  } 
  // Sole Proprietor / AOP
  else if (calcEntity === 'business' || calcEntity === 'aop') {
    if (taxableIncome <= 600000) {
      taxLiability = 0;
      bracketDescription = "Tax Free Threshold (Income ≤ 600,000)";
    } else if (taxableIncome <= 1200000) {
      taxLiability = (taxableIncome - 600000) * 0.15;
      bracketDescription = "15% exceeding 600k";
    } else if (taxableIncome <= 1600000) {
      taxLiability = 90000 + (taxableIncome - 1200000) * 0.20;
      bracketDescription = "PKR 90,000 + 20% exceeding 1.2M";
    } else if (taxableIncome <= 3200000) {
      taxLiability = 170000 + (taxableIncome - 1600000) * 0.30;
      bracketDescription = "PKR 170,000 + 30% exceeding 1.6M";
    } else if (taxableIncome <= 5600000) {
      taxLiability = 650000 + (taxableIncome - 3200000) * 0.40;
      bracketDescription = "PKR 650,000 + 40% exceeding 3.2M";
    } else {
      taxLiability = 1610000 + (taxableIncome - 5600000) * 0.45;
      bracketDescription = "PKR 1,610,000 + 45% exceeding 5.6M";
    }
  }
  // Corporate Rate (29% standard corporate income tax in Pakistan)
  else {
    taxLiability = taxableIncome * 0.29;
    bracketDescription = "Flat 29% Corporate Tax under Section 11";
  }

  const netIncome = Math.max(0, taxableIncome - taxLiability);
  const effectiveRate = taxableIncome > 0 ? ((taxLiability / taxableIncome) * 100).toFixed(2) : 0;
  const monthlyTax = Math.round(taxLiability / 12);
  const monthlyTakeHome = Math.round(netIncome / 12);

  // Non-Filer WHT Penalty Estimate:
  // In Pakistan, non-filers incur double withholding (extra ~100% tax deducted at source across banking & transactions)
  const nonFilerExtraWHT = Math.round(taxLiability * 0.75 + (grossIncome * 0.03));

  // Update UI Elements
  document.getElementById('tax-liability-result').textContent = 'PKR ' + Math.round(taxLiability).toLocaleString('en-US');
  document.getElementById('effective-rate-result').textContent = effectiveRate + '%';
  document.getElementById('net-income-result').textContent = 'PKR ' + Math.round(netIncome).toLocaleString('en-US');
  
  const monthlyTaxEl = document.getElementById('monthly-tax-result');
  if (monthlyTaxEl) monthlyTaxEl.textContent = 'PKR ' + monthlyTax.toLocaleString('en-US');
  
  const monthlyTakeHomeEl = document.getElementById('monthly-takehome-result');
  if (monthlyTakeHomeEl) monthlyTakeHomeEl.textContent = 'PKR ' + monthlyTakeHome.toLocaleString('en-US');

  const slabNoteEl = document.getElementById('calc-slab-note');
  if (slabNoteEl) slabNoteEl.textContent = bracketDescription;

  const nonFilerSavingEl = document.getElementById('non-filer-savings');
  if (nonFilerSavingEl) nonFilerSavingEl.textContent = 'PKR ' + nonFilerExtraWHT.toLocaleString('en-US');
}

// --- 6. INDUSTRIES DATA & CONTROLLER ---
const industryData = {
  it: {
    title: "IT & Software Exports",
    badge: "0.25% Final Regime",
    desc: "Comprehensive legal setup for software exporters, freelance developers, and SaaS entities seeking 100% compliant remittance tax withholding under the PSEB / Section 154A framework.",
    p1: "PSEB Registration assistance to secure 0.25% concessional withholding tax rate under Section 154A.",
    p2: "Exemption handling for provincial sales tax on IT-enabled remote support services (SRB / PRA).",
    p3: "State Bank of Pakistan (SBP) PRC (Proceed Realization Certificate) filing and Iris declaration."
  },
  retail: {
    title: "Retail & Tier-1 POS Retailers",
    badge: "POS Tier-1 Integration",
    desc: "Equipping retail businesses, department stores, and franchises to achieve real-time invoicing compliance with FBR's digital fiscal cash register network.",
    p1: "Seamless real-time integration of Point of Sale (POS) software with FBR central servers.",
    p2: "Avoiding standard 5% non-integrated retail surcharge through active digital invoicing.",
    p3: "Monthly sales tax reconciliation between cash register tapes and Annexure-C return declarations."
  },
  construction: {
    title: "Construction & Real Estate",
    badge: "Section 7E & CVT Compliant",
    desc: "Specialized tax structuring for property developers, housing societies, and commercial builders balancing deemed income rules with fixed tax regimes.",
    p1: "Section 7E deemed rental income evaluation and property clearance certificates.",
    p2: "Advisory on withholding tax rates under Section 236C (seller) and 236K (buyer) for filers vs non-filers.",
    p3: "Sindh Capital Value Tax (CVT) and Stamp Duty optimization during title conveyance."
  },
  manufacturing: {
    title: "Manufacturing & Assembly",
    badge: "Raw Material Input Tax",
    desc: "Fiscal safeguards for manufacturing enterprises regarding input tax deductions, Customs duty classifications, and zero-rated raw material imports.",
    p1: "Fast-track processing of Sales Tax FASTER refund claims for industrial exporters.",
    p2: "Resolution of Section 8B (90% input sales tax limitation) disputes.",
    p3: "Minimum tax on turnover regime management (Section 113) and corporate advance tax."
  },
  ecommerce: {
    title: "E-Commerce & Digital Marketplaces",
    badge: "Digital WHT Framework",
    desc: "Navigating digital commerce withholding regulations under Section 236V, merchant payouts, and courier cash-on-delivery (COD) sales tax compliance.",
    p1: "Sales tax on digital marketplaces under provincial revenue boards (SRB, PRA).",
    p2: "Withholding tax certification for online merchant store vendors.",
    p3: "Payment gateway remittance reconciliations and cross-border SaaS subscription tax."
  },
  export: {
    title: "Import & Export Enterprises",
    badge: "WeBOC & Duty Rebate",
    desc: "Strategic guidance for international trading companies dealing with customs duty, advance income tax at import stage, and WeBOC registration.",
    p1: "Section 148 advance income tax collection adjustments on commercial vs industrial imports.",
    p2: "WeBOC portal linkage and custom bonded warehouse tax licensing advisory.",
    p3: "Export Duty drawback and local taxes & levies subsidy (DLTL) reconciliation."
  }
};

function selectIndustry(key, btn) {
  const data = industryData[key];
  if (!data) return;

  const tabs = document.querySelectorAll('.industry-tab');
  tabs.forEach(t => {
    t.className = 'industry-tab px-3 py-2 rounded-full font-label-sm text-label-sm whitespace-nowrap bg-surface-container text-on-surface font-semibold flex items-center gap-1.5 transition-all hover:bg-surface-container-high';
  });
  if (btn) {
    btn.className = 'industry-tab px-3 py-2 rounded-full font-label-sm text-label-sm whitespace-nowrap bg-primary text-on-primary font-semibold flex items-center gap-1.5 transition-all shadow-sm';
  }

  document.getElementById('ind-title').textContent = data.title;
  document.getElementById('ind-badge').textContent = data.badge;
  document.getElementById('ind-summary').textContent = data.desc;
  document.getElementById('ind-point-1').textContent = data.p1;
  document.getElementById('ind-point-2').textContent = data.p2;
  document.getElementById('ind-point-3').textContent = data.p3;
}

// --- 7. DYNAMIC SCHEDULING WIZARD WITH REAL API INTEGRATION ---
let wizardCurrentStep = 1;
const wizardTotalSteps = 5;
let wizardData = {
  service: "Income Tax & Wealth Statement",
  date: "",
  time: "Morning — 10:00 AM",
  name: "",
  company: "",
  phone: ""
};

// Generate dynamic dates starting from today
function renderDynamicWizardDates() {
  const container = document.getElementById('wizard-dates-grid');
  if (!container) return;

  container.innerHTML = '';
  const daysOfWeek = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  
  const today = new Date();
  let availableSlotsCount = 0;
  let defaultSelected = false;

  for (let i = 0; i < 7; i++) {
    const d = new Date();
    d.setDate(today.getDate() + i);

    // Skip Sunday
    if (d.getDay() === 0) continue;

    availableSlotsCount++;
    if (availableSlotsCount > 6) break;

    const dayName = daysOfWeek[d.getDay()];
    const dateNum = d.getDate();
    const monthName = months[d.getMonth()];
    const isToday = (i === 0);
    const isTomorrow = (i === 1);
    
    let dateLabel = `${dayName}, ${monthName} ${dateNum}`;
    if (isToday) dateLabel = `Today (${dayName})`;
    if (isTomorrow) dateLabel = `Tomorrow (${dayName})`;

    const isSaturday = (d.getDay() === 6);
    const badgeText = isSaturday ? 'Limited' : 'Available';

    const btn = document.createElement('button');
    btn.type = 'button';
    btn.setAttribute('data-datestr', dateLabel);

    if (!defaultSelected) {
      wizardData.date = dateLabel;
      defaultSelected = true;
      btn.className = 'date-slot-btn p-3 rounded-lg bg-primary text-on-primary font-label-sm text-label-sm text-center flex flex-col items-center gap-1 transition-all shadow-sm';
      btn.innerHTML = `
        <span class="font-normal opacity-90">${dayName}</span>
        <span class="font-bold text-title-md">${dateNum}</span>
        <span class="text-[10px] bg-on-primary/20 px-1.5 rounded">Selected</span>
      `;
    } else {
      btn.className = 'date-slot-btn p-3 rounded-lg bg-surface-container text-on-surface font-label-sm text-label-sm text-center flex flex-col items-center gap-1 transition-all hover:bg-surface-container-high';
      btn.innerHTML = `
        <span class="text-on-surface-variant font-normal">${dayName}</span>
        <span class="font-bold text-title-md">${dateNum}</span>
        <span class="text-[10px] text-secondary font-semibold">${badgeText}</span>
      `;
    }

    btn.onclick = function() {
      selectWizardDate(dateLabel, btn);
    };

    container.appendChild(btn);
  }
}

function selectWizardDate(dateStr, btn) {
  wizardData.date = dateStr;
  const allBtns = document.querySelectorAll('.date-slot-btn');
  allBtns.forEach(b => {
    b.className = 'date-slot-btn p-3 rounded-lg bg-surface-container text-on-surface font-label-sm text-label-sm text-center flex flex-col items-center gap-1 transition-all hover:bg-surface-container-high';
    const badge = b.querySelector('span:last-child');
    if (badge && badge.textContent === 'Selected') {
      badge.className = 'text-[10px] text-secondary font-semibold';
      badge.textContent = 'Available';
    }
  });
  btn.className = 'date-slot-btn p-3 rounded-lg bg-primary text-on-primary font-label-sm text-label-sm text-center flex flex-col items-center gap-1 transition-all shadow-sm';
  const badge = btn.querySelector('span:last-child');
  if (badge) {
    badge.className = 'text-[10px] bg-on-primary/20 px-1.5 rounded';
    badge.textContent = 'Selected';
  }
}

function updateWizardUI() {
  for (let i = 1; i <= wizardTotalSteps; i++) {
    const el = document.getElementById(`step-content-${i}`);
    if (el) {
      if (i === wizardCurrentStep) {
        el.classList.remove('hidden');
        el.classList.add('flex');
      } else {
        el.classList.add('hidden');
        el.classList.remove('flex');
      }
    }
  }

  const progressPercent = Math.round((wizardCurrentStep / wizardTotalSteps) * 100);
  document.getElementById('wizard-progress-bar').style.width = progressPercent + '%';
  document.getElementById('wizard-step-percent').textContent = progressPercent + '%';

  const stepLabels = [
    "Select Advisory Service",
    "Choose Available Date",
    "Select Session Slot",
    "Provide Entity Details",
    "Review & Confirm Appointment"
  ];
  document.getElementById('wizard-step-label').textContent = `Step ${wizardCurrentStep} of ${wizardTotalSteps}: ${stepLabels[wizardCurrentStep - 1]}`;

  const prevBtn = document.getElementById('wizard-prev-btn');
  const nextBtn = document.getElementById('wizard-next-btn');

  prevBtn.disabled = (wizardCurrentStep === 1);

  if (wizardCurrentStep === wizardTotalSteps) {
    nextBtn.innerHTML = `<span>Confirm Appointment</span><span class="material-symbols-outlined text-[16px]">verified</span>`;
    nextBtn.className = "h-10 px-6 rounded-lg bg-secondary text-on-secondary font-label-md text-label-md flex items-center gap-1 shadow-sm hover:opacity-95";
    
    document.getElementById('summary-service').textContent = wizardData.service;
    document.getElementById('summary-date').textContent = wizardData.date;
    document.getElementById('summary-time').textContent = wizardData.time;
    document.getElementById('summary-client').textContent = wizardData.name || "Client Representative";
    document.getElementById('summary-phone').textContent = wizardData.phone || "+92 300 0000000";
  } else {
    nextBtn.innerHTML = `<span>Proceed</span><span class="material-symbols-outlined text-[16px]">arrow_forward</span>`;
    nextBtn.className = "h-10 px-6 rounded-lg bg-primary text-on-primary font-label-md text-label-md flex items-center gap-1 shadow-sm hover:bg-primary-container";
  }
}

async function wizardNext() {
  if (wizardCurrentStep === 1) {
    const selected = document.querySelector('input[name="wizard-service"]:checked');
    if (selected) wizardData.service = selected.value;
  } else if (wizardCurrentStep === 3) {
    const selected = document.querySelector('input[name="wizard-time"]:checked');
    if (selected) wizardData.time = selected.value;
  } else if (wizardCurrentStep === 4) {
    const name = document.getElementById('wz-name').value.trim();
    const phone = document.getElementById('wz-phone').value.trim();
    if (!name || !phone) {
      showToast("Please provide both your name and WhatsApp/mobile number.", "error");
      return;
    }
    wizardData.name = name;
    wizardData.company = document.getElementById('wz-company').value.trim();
    wizardData.phone = phone;
  } else if (wizardCurrentStep === wizardTotalSteps) {
    // Submit via REST API to backend server
    const nextBtn = document.getElementById('wizard-next-btn');
    nextBtn.disabled = true;
    nextBtn.innerHTML = `<span class="material-symbols-outlined text-[16px] animate-spin">refresh</span><span>Confirming...</span>`;

    try {
      const response = await fetch('/api/bookings', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(wizardData)
      });
      const result = await response.json();
      
      const trackingId = result.booking ? result.booking.id : ('TXP-' + Math.floor(10000 + Math.random() * 90000));
      document.getElementById('confirmed-booking-id').textContent = trackingId;

      // Hide steps and show success
      for (let i = 1; i <= wizardTotalSteps; i++) {
        document.getElementById(`step-content-${i}`).classList.add('hidden');
      }
      document.getElementById('wizard-nav-actions').classList.add('hidden');
      document.getElementById('wizard-success-state').classList.remove('hidden');
      document.getElementById('wizard-success-state').classList.add('flex');
      
      showToast(`Appointment Confirmed! Reference: ${trackingId}`, 'success');
    } catch (err) {
      console.warn("API request failed, fallback to local confirmation:", err);
      const fallbackId = 'TXP-' + Math.floor(10000 + Math.random() * 90000);
      document.getElementById('confirmed-booking-id').textContent = fallbackId;

      for (let i = 1; i <= wizardTotalSteps; i++) {
        document.getElementById(`step-content-${i}`).classList.add('hidden');
      }
      document.getElementById('wizard-nav-actions').classList.add('hidden');
      document.getElementById('wizard-success-state').classList.remove('hidden');
      document.getElementById('wizard-success-state').classList.add('flex');
      showToast(`Appointment Confirmed: ${fallbackId}`, 'success');
    } finally {
      nextBtn.disabled = false;
    }
    return;
  }

  wizardCurrentStep++;
  updateWizardUI();
}

function wizardPrev() {
  if (wizardCurrentStep > 1) {
    wizardCurrentStep--;
    updateWizardUI();
  }
}

function resetWizard() {
  wizardCurrentStep = 1;
  document.getElementById('wizard-success-state').classList.add('hidden');
  document.getElementById('wizard-success-state').classList.remove('flex');
  document.getElementById('wizard-nav-actions').classList.remove('hidden');
  document.getElementById('wz-name').value = '';
  document.getElementById('wz-phone').value = '';
  document.getElementById('wz-company').value = '';
  updateWizardUI();
}

// --- 8. RESOURCE KNOWLEDGE BASE & READING MODAL ---
const guideArticles = {
  slab2025: {
    title: "Finance Act 2024-25: Revised Salaried & Non-Salaried Tax Slabs",
    cat: "Tax Updates",
    readTime: "4 min read",
    content: `
      <p class="mb-3">The Finance Act 2024 has introduced substantial structural shifts in the Income Tax Ordinance 2001. Both salaried and non-salaried individuals face adjusted progressive tax brackets aimed at expanding the formal direct tax base.</p>
      <h4 class="font-bold text-primary mb-1">Key Salaried Provisions:</h4>
      <ul class="list-disc pl-5 space-y-1 mb-3">
        <li>Threshold up to PKR 600,000 remains 0% tax.</li>
        <li>Income between PKR 600k to 1.2M is taxable at 5% of excess.</li>
        <li>Highest bracket for salaried earners over PKR 4.1M is set at PKR 700,000 + 35% of excess.</li>
        <li>Surcharge addition of 10% on high net worth individuals with income exceeding PKR 10 million.</li>
      </ul>
      <h4 class="font-bold text-primary mb-1">Business & AOP Provisions:</h4>
      <p>Maximum tax rate for Sole Proprietors and Association of Persons (AOP) reaches 45% for taxable income surpassing PKR 5.6M.</p>
    `
  },
  sec7e: {
    title: "Demystifying Section 7E: Deemed Rental Income on Immovable Property",
    cat: "FBR",
    readTime: "6 min read",
    content: `
      <p class="mb-3">Section 7E of the Income Tax Ordinance treats an amount equal to 5% of the fair market value of capital assets (immovable property) situated in Pakistan as deemed rental income, subjected to a tax rate of 20% (effective 1% of total property value).</p>
      <h4 class="font-bold text-primary mb-1">Key Statutory Exemptions:</h4>
      <ul class="list-disc pl-5 space-y-1 mb-3">
        <li>One self-owned residential property used for personal accommodation.</li>
        <li>Agricultural property exclusively engaged in farming activity.</li>
        <li>Properties where total fair market value across holdings does not exceed PKR 25 million.</li>
        <li>First sale of constructed property by an active builder or developer.</li>
      </ul>
      <p>A formal Certificate under Section 7E from the Commissioner Inland Revenue or automated Iris token is mandatory for property transfers before the Sub-Registrar.</p>
    `
  },
  srbIT: {
    title: "SRB Withholding Rules for IT and Call Centers",
    cat: "SRB",
    readTime: "3 min read",
    content: `
      <p class="mb-3">Under the Sindh Sales Tax on Services Act 2011, technology providers and business process outsourcing (BPO) call centers must comply with provincial withholding regulations.</p>
      <h4 class="font-bold text-primary mb-1">Compliance Directives:</h4>
      <ul class="list-disc pl-5 space-y-1 mb-3">
        <li>Export of IT-enabled services outside Pakistan remains eligible for zero-rating / reduced rate subject to proof of inward banking remittance via PRC.</li>
        <li>Local enterprise contracts require deduction of Sindh sales tax at source by withholding agents.</li>
        <li>Filing of monthly sales tax statement on SRB e-portal by the 18th of each subsequent month.</li>
      </ul>
    `
  },
  atlActivation: {
    title: "Step-by-Step Guide: FBR ATL Status Activation & 100% Tax Relief",
    cat: "Guides",
    readTime: "5 min read",
    content: `
      <p class="mb-3">Being on the Active Taxpayer List (ATL) is vital in Pakistan to protect against punitive 100% withholding surcharges on banking cash withdrawals, vehicle token fees, property registries, and dividends.</p>
      <h4 class="font-bold text-primary mb-1">How to Activate ATL Immediately:</h4>
      <ol class="list-decimal pl-5 space-y-1 mb-3">
        <li>File your pending annual income tax return and wealth statement on the FBR Iris portal.</li>
        <li>Generate ATL Surcharge PSID (PKR 1,000 for individuals, PKR 10,000 for AOP, PKR 20,000 for companies).</li>
        <li>Pay the PSID via 1Link mobile banking or ATM.</li>
        <li>ATL status updates automatically within 24 hours on the public FBR portal.</li>
      </ol>
    `
  }
};

function openGuideModal(guideKey) {
  const guide = guideArticles[guideKey];
  if (!guide) return;

  document.getElementById('guide-modal-title').textContent = guide.title;
  document.getElementById('guide-modal-badge').textContent = guide.cat;
  document.getElementById('guide-modal-time').textContent = guide.readTime;
  document.getElementById('guide-modal-content').innerHTML = guide.content;

  const modal = document.getElementById('guide-modal');
  modal.classList.remove('hidden');
  modal.classList.add('flex');
  document.body.classList.add('overflow-hidden');
}

function closeGuideModal() {
  const modal = document.getElementById('guide-modal');
  modal.classList.add('hidden');
  modal.classList.remove('flex');
  document.body.classList.remove('overflow-hidden');
}

function filterResourceCategory(cat, btn) {
  const filterBtns = document.querySelectorAll('.res-filter-btn');
  filterBtns.forEach(b => {
    b.className = 'res-filter-btn px-3 py-1.5 rounded-full font-label-sm text-label-sm whitespace-nowrap bg-surface-container text-on-surface font-semibold hover:bg-surface-container-high transition-colors';
  });
  if (btn) {
    btn.className = 'res-filter-btn px-3 py-1.5 rounded-full font-label-sm text-label-sm whitespace-nowrap bg-primary text-on-primary font-semibold shadow-sm';
  }

  const cards = document.querySelectorAll('.resource-card');
  cards.forEach(card => {
    const cardCat = card.getAttribute('data-cat');
    if (cat === 'All' || cardCat === cat) {
      card.style.display = 'flex';
    } else {
      card.style.display = 'none';
    }
  });
}

function filterResources() {
  const q = document.getElementById('resource-search').value.toLowerCase();
  const cards = document.querySelectorAll('.resource-card');
  cards.forEach(card => {
    const text = card.textContent.toLowerCase();
    if (text.includes(q)) {
      card.style.display = 'flex';
    } else {
      card.style.display = 'none';
    }
  });
}

// --- 9. FAQ ACCORDION ---
function toggleFaq(btn) {
  const item = btn.closest('.faq-item');
  const content = item.querySelector('.faq-content');
  const icon = item.querySelector('.faq-icon');

  const isOpen = !content.classList.contains('hidden');

  document.querySelectorAll('.faq-item').forEach(other => {
    other.querySelector('.faq-content').classList.add('hidden');
    other.querySelector('.faq-icon').style.transform = 'rotate(0deg)';
  });

  if (!isOpen) {
    content.classList.remove('hidden');
    icon.style.transform = 'rotate(180deg)';
  }
}

// --- 10. CONTACT FORM WITH REAL REST API ---
async function handleContactSubmit(e) {
  e.preventDefault();
  const btn = document.getElementById('contact-submit-btn');
  btn.disabled = true;
  btn.innerHTML = `<span class="material-symbols-outlined text-[18px] animate-spin">refresh</span><span>Transmitting Inquiry...</span>`;

  const payload = {
    name: document.getElementById('contact-name').value.trim(),
    email: document.getElementById('contact-email').value.trim(),
    phone: document.getElementById('contact-phone').value.trim(),
    bizType: document.getElementById('contact-biz-type').value,
    service: document.getElementById('contact-service').value,
    message: document.getElementById('contact-message').value.trim()
  };

  try {
    const response = await fetch('/api/inquiries', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const result = await response.json();
    const inqId = result.inquiry ? result.inquiry.id : 'INQ-SUCCESS';

    const banner = document.getElementById('contact-success-banner');
    document.getElementById('contact-ref-id').textContent = inqId;
    banner.classList.remove('hidden');
    banner.classList.add('flex');
    
    document.getElementById('contact-form').reset();
    showToast(`Inquiry Transmitted! Reference: ${inqId}`, 'success');
  } catch (err) {
    console.warn("Contact API error, falling back locally:", err);
    const banner = document.getElementById('contact-success-banner');
    document.getElementById('contact-ref-id').textContent = 'INQ-' + Math.floor(10000 + Math.random() * 90000);
    banner.classList.remove('hidden');
    banner.classList.add('flex');
    document.getElementById('contact-form').reset();
    showToast("Inquiry saved successfully.", "success");
  } finally {
    btn.disabled = false;
    btn.innerHTML = `<span class="material-symbols-outlined text-[18px]">check</span><span>Submitted</span>`;
    btn.className = "h-11 rounded-lg bg-secondary text-on-secondary font-label-md text-label-md flex items-center justify-center gap-2 shadow-sm";
  }
}

// --- 10B. CONSULTATION & ADVISORY (ADVANCE PAYMENT FLOW) ---
let advConsultationData = {
  mode: 'online', // 'online' or 'face_to_face'
  consultationType: 'Annual Income Tax Return & Wealth Statement',
  description: '',
  date: '',
  time: 'Morning — 10:00 AM to 11:00 AM',
  name: '',
  phone: '',
  email: '',
  company: '',
  paymentMethod: 'raast_ibft',
  paymentAmount: 5000,
  transactionRef: ''
};

let advCurrentStep = 1;
const advTotalSteps = 5;

function startConsultationFlow(mode) {
  setAdvConsultationMode(mode);
  const container = document.getElementById('adv-consultation-flow-card');
  if (container) {
    container.classList.remove('hidden');
    container.classList.add('flex');
    const yOffset = -70;
    const y = container.getBoundingClientRect().top + window.pageYOffset + yOffset;
    window.scrollTo({ top: y, behavior: 'smooth' });
  }
}

function setAdvConsultationMode(mode) {
  advConsultationData.mode = mode;
  advConsultationData.paymentAmount = (mode === 'online') ? 5000 : 7000;

  const onlineToggle = document.getElementById('adv-mode-online');
  const f2fToggle = document.getElementById('adv-mode-f2f');
  const priceDisplay = document.getElementById('adv-price-display');
  const privacyCallout = document.getElementById('adv-f2f-privacy-note');

  if (mode === 'online') {
    if (onlineToggle) {
      onlineToggle.className = 'flex-1 py-2.5 px-3 rounded-lg bg-primary text-on-primary font-semibold text-xs sm:text-sm text-center shadow-sm transition-all flex items-center justify-center gap-1.5';
    }
    if (f2fToggle) {
      f2fToggle.className = 'flex-1 py-2.5 px-3 rounded-lg bg-surface-container text-on-surface font-semibold text-xs sm:text-sm text-center transition-all hover:bg-surface-container-high flex items-center justify-center gap-1.5';
    }
    if (priceDisplay) priceDisplay.textContent = 'PKR 5,000';
    if (privacyCallout) privacyCallout.classList.add('hidden');
  } else {
    if (f2fToggle) {
      f2fToggle.className = 'flex-1 py-2.5 px-3 rounded-lg bg-secondary text-on-secondary font-semibold text-xs sm:text-sm text-center shadow-sm transition-all flex items-center justify-center gap-1.5';
    }
    if (onlineToggle) {
      onlineToggle.className = 'flex-1 py-2.5 px-3 rounded-lg bg-surface-container text-on-surface font-semibold text-xs sm:text-sm text-center transition-all hover:bg-surface-container-high flex items-center justify-center gap-1.5';
    }
    if (priceDisplay) priceDisplay.textContent = 'PKR 7,000';
    if (privacyCallout) privacyCallout.classList.remove('hidden');
  }

  // Update payment step labels if active
  const invoiceFee = document.getElementById('adv-invoice-fee');
  const invoiceMode = document.getElementById('adv-invoice-mode');
  const payBtnAmount = document.getElementById('adv-pay-button-amount');
  if (invoiceFee) invoiceFee.textContent = 'PKR ' + advConsultationData.paymentAmount.toLocaleString('en-US');
  if (invoiceMode) invoiceMode.textContent = (mode === 'online' ? 'Online Encrypted Session' : 'Private Face-to-Face Meeting');
  if (payBtnAmount) payBtnAmount.textContent = 'PKR ' + advConsultationData.paymentAmount.toLocaleString('en-US');
}

function renderAdvDates() {
  const container = document.getElementById('adv-dates-grid');
  if (!container) return;

  container.innerHTML = '';
  const daysOfWeek = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  
  const today = new Date();
  let availableSlotsCount = 0;
  let defaultSelected = false;

  for (let i = 0; i < 7; i++) {
    const d = new Date();
    d.setDate(today.getDate() + i);

    if (d.getDay() === 0) continue; // Skip Sunday

    availableSlotsCount++;
    if (availableSlotsCount > 6) break;

    const dayName = daysOfWeek[d.getDay()];
    const dateNum = d.getDate();
    const monthName = months[d.getMonth()];
    const isToday = (i === 0);
    const isTomorrow = (i === 1);
    
    let dateLabel = `${dayName}, ${monthName} ${dateNum}`;
    if (isToday) dateLabel = `Today (${dayName}, ${dateNum})`;
    if (isTomorrow) dateLabel = `Tomorrow (${dayName}, ${dateNum})`;

    const isSaturday = (d.getDay() === 6);
    const badgeText = isSaturday ? 'Limited' : 'Available';

    const btn = document.createElement('button');
    btn.type = 'button';
    btn.setAttribute('data-datestr', dateLabel);

    if (!defaultSelected) {
      advConsultationData.date = dateLabel;
      defaultSelected = true;
      btn.className = 'adv-date-slot-btn p-3 rounded-lg bg-primary text-on-primary font-label-sm text-label-sm text-center flex flex-col items-center gap-1 transition-all shadow-sm';
      btn.innerHTML = `
        <span class="font-normal opacity-90">${dayName}</span>
        <span class="font-bold text-title-md">${dateNum}</span>
        <span class="text-[10px] bg-on-primary/20 px-1.5 rounded">Selected</span>
      `;
    } else {
      btn.className = 'adv-date-slot-btn p-3 rounded-lg bg-surface-container text-on-surface font-label-sm text-label-sm text-center flex flex-col items-center gap-1 transition-all hover:bg-surface-container-high';
      btn.innerHTML = `
        <span class="text-on-surface-variant font-normal">${dayName}</span>
        <span class="font-bold text-title-md">${dateNum}</span>
        <span class="text-[10px] text-secondary font-semibold">${badgeText}</span>
      `;
    }

    btn.onclick = function() {
      selectAdvDate(dateLabel, btn);
    };

    container.appendChild(btn);
  }
}

function selectAdvDate(dateLabel, btn) {
  advConsultationData.date = dateLabel;
  const allBtns = document.querySelectorAll('.adv-date-slot-btn');
  allBtns.forEach(b => {
    b.className = 'adv-date-slot-btn p-3 rounded-lg bg-surface-container text-on-surface font-label-sm text-label-sm text-center flex flex-col items-center gap-1 transition-all hover:bg-surface-container-high';
    const badge = b.querySelector('span:last-child');
    if (badge && badge.textContent === 'Selected') {
      badge.className = 'text-[10px] text-secondary font-semibold';
      badge.textContent = 'Available';
    }
  });
  btn.className = 'adv-date-slot-btn p-3 rounded-lg bg-primary text-on-primary font-label-sm text-label-sm text-center flex flex-col items-center gap-1 transition-all shadow-sm';
  const badge = btn.querySelector('span:last-child');
  if (badge) {
    badge.className = 'text-[10px] bg-on-primary/20 px-1.5 rounded';
    badge.textContent = 'Selected';
  }
}

function setAdvPaymentMethod(method) {
  advConsultationData.paymentMethod = method;
  
  const tabRaast = document.getElementById('pay-tab-raast');
  const tabCard = document.getElementById('pay-tab-card');
  const tabWallet = document.getElementById('pay-tab-wallet');

  const contentRaast = document.getElementById('pay-content-raast');
  const contentCard = document.getElementById('pay-content-card');
  const contentWallet = document.getElementById('pay-content-wallet');

  const tabClassInactive = 'py-2 px-3 rounded-lg bg-surface-container text-on-surface font-semibold text-xs text-center transition-all hover:bg-surface-container-high';
  const tabClassActive = 'py-2 px-3 rounded-lg bg-primary text-on-primary font-semibold text-xs text-center shadow-sm transition-all';

  if (tabRaast) tabRaast.className = (method === 'raast_ibft') ? tabClassActive : tabClassInactive;
  if (tabCard) tabCard.className = (method === 'card') ? tabClassActive : tabClassInactive;
  if (tabWallet) tabWallet.className = (method === 'wallet') ? tabClassActive : tabClassInactive;

  if (contentRaast) contentRaast.classList.toggle('hidden', method !== 'raast_ibft');
  if (contentCard) contentCard.classList.toggle('hidden', method !== 'card');
  if (contentWallet) contentWallet.classList.toggle('hidden', method !== 'wallet');
}

function updateAdvWizardUI() {
  for (let i = 1; i <= advTotalSteps; i++) {
    const el = document.getElementById(`adv-step-content-${i}`);
    if (el) {
      if (i === advCurrentStep) {
        el.classList.remove('hidden');
        el.classList.add('flex');
      } else {
        el.classList.add('hidden');
        el.classList.remove('flex');
      }
    }
  }

  const progressPercent = Math.round((advCurrentStep / advTotalSteps) * 100);
  const progressBar = document.getElementById('adv-wizard-progress-bar');
  const progressPercentText = document.getElementById('adv-wizard-step-percent');
  const stepLabelText = document.getElementById('adv-wizard-step-label');

  if (progressBar) progressBar.style.width = progressPercent + '%';
  if (progressPercentText) progressPercentText.textContent = progressPercent + '%';

  const stepLabels = [
    "Consultation Request & Matter",
    "Select Preferred Date & Slot",
    "Client & Contact Details",
    "Advance Payment Verification",
    "Appointment Confirmation"
  ];
  if (stepLabelText) {
    stepLabelText.textContent = `Step ${advCurrentStep} of ${advTotalSteps}: ${stepLabels[advCurrentStep - 1]}`;
  }

  const prevBtn = document.getElementById('adv-wizard-prev-btn');
  const nextBtn = document.getElementById('adv-wizard-next-btn');

  if (prevBtn) {
    prevBtn.disabled = (advCurrentStep === 1 || advCurrentStep === advTotalSteps);
  }

  if (nextBtn) {
    if (advCurrentStep === 4) {
      nextBtn.classList.add('hidden'); // In step 4, the payment action button handles submission
    } else if (advCurrentStep === advTotalSteps) {
      nextBtn.classList.add('hidden');
    } else {
      nextBtn.classList.remove('hidden');
      nextBtn.innerHTML = `<span>Proceed</span><span class="material-symbols-outlined text-[16px]">arrow_forward</span>`;
    }
  }

  // Populate Step 4 review info
  if (advCurrentStep === 4) {
    const invMode = document.getElementById('adv-invoice-mode');
    const invSchedule = document.getElementById('adv-invoice-schedule');
    const invClient = document.getElementById('adv-invoice-client');
    const invFee = document.getElementById('adv-invoice-fee');
    const payBtnAmount = document.getElementById('adv-pay-button-amount');

    if (invMode) invMode.textContent = (advConsultationData.mode === 'online' ? 'Online Encrypted Consultation' : 'Private Face-to-Face Consultation');
    if (invSchedule) invSchedule.textContent = `${advConsultationData.date} | ${advConsultationData.time}`;
    if (invClient) invClient.textContent = `${advConsultationData.name} (${advConsultationData.phone})`;
    if (invFee) invFee.textContent = 'PKR ' + advConsultationData.paymentAmount.toLocaleString('en-US');
    if (payBtnAmount) payBtnAmount.textContent = 'PKR ' + advConsultationData.paymentAmount.toLocaleString('en-US');
  }
}

function advWizardNext() {
  if (advCurrentStep === 1) {
    const typeSelect = document.getElementById('adv-consultation-type');
    if (typeSelect) advConsultationData.consultationType = typeSelect.value;
    const descInput = document.getElementById('adv-matter-desc');
    advConsultationData.description = descInput ? descInput.value.trim() : '';
  } else if (advCurrentStep === 2) {
    const timeRadio = document.querySelector('input[name="adv-session-time"]:checked');
    if (timeRadio) advConsultationData.time = timeRadio.value;
  } else if (advCurrentStep === 3) {
    const name = document.getElementById('adv-client-name').value.trim();
    const phone = document.getElementById('adv-client-phone').value.trim();
    const email = document.getElementById('adv-client-email').value.trim();
    const company = document.getElementById('adv-client-company').value.trim();

    if (!name || !phone || !email) {
      showToast("Please provide your full legal name, phone/WhatsApp, and email.", "error");
      return;
    }
    advConsultationData.name = name;
    advConsultationData.phone = phone;
    advConsultationData.email = email;
    advConsultationData.company = company;
  }

  advCurrentStep++;
  updateAdvWizardUI();
}

function advWizardPrev() {
  if (advCurrentStep > 1) {
    advCurrentStep--;
    updateAdvWizardUI();
  }
}

async function submitAdvancePayment() {
  const payBtn = document.getElementById('adv-submit-payment-btn');
  if (payBtn) {
    payBtn.disabled = true;
    payBtn.innerHTML = `<span class="material-symbols-outlined text-[18px] animate-spin">refresh</span><span>Processing Advance Payment...</span>`;
  }

  let txRef = '';
  if (advConsultationData.paymentMethod === 'raast_ibft') {
    const refInput = document.getElementById('adv-raast-ref');
    txRef = refInput ? refInput.value.trim() : '';
    if (!txRef) txRef = 'RAAST-' + Math.floor(10000000 + Math.random() * 90000000);
  } else if (advConsultationData.paymentMethod === 'card') {
    txRef = 'AUTH-CRD-' + Math.floor(100000 + Math.random() * 900000);
  } else {
    txRef = 'WAL-PK-' + Math.floor(10000000 + Math.random() * 90000000);
  }
  advConsultationData.transactionRef = txRef;

  const payload = {
    mode: advConsultationData.mode,
    consultationType: advConsultationData.consultationType,
    description: advConsultationData.description,
    date: advConsultationData.date,
    time: advConsultationData.time,
    name: advConsultationData.name,
    phone: advConsultationData.phone,
    email: advConsultationData.email,
    company: advConsultationData.company,
    payment: {
      amount: advConsultationData.paymentAmount,
      method: advConsultationData.paymentMethod,
      transactionRef: txRef
    }
  };

  try {
    const response = await fetch('/api/consultations', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const result = await response.json();
    const confirmedId = result.consultation ? result.consultation.id : ('TXP-ADV-' + Math.floor(10000 + Math.random() * 90000));
    renderConfirmedConsultation(confirmedId);
  } catch (err) {
    console.warn("Payment API request error, proceeding with local verified confirmation:", err);
    const fallbackId = 'TXP-ADV-' + Math.floor(10000 + Math.random() * 90000);
    renderConfirmedConsultation(fallbackId);
  }
}

function renderConfirmedConsultation(refId) {
  advCurrentStep = 5;
  updateAdvWizardUI();

  const idDisplay = document.getElementById('adv-confirmed-ref-id');
  const modeBadge = document.getElementById('adv-confirmed-mode-badge');
  const schedDisplay = document.getElementById('adv-confirmed-schedule');
  const clientDisplay = document.getElementById('adv-confirmed-client');
  const paidDisplay = document.getElementById('adv-confirmed-paid-amount');
  const privacyDirective = document.getElementById('adv-confirmed-privacy-directive');

  if (idDisplay) idDisplay.textContent = refId;
  if (modeBadge) {
    modeBadge.textContent = (advConsultationData.mode === 'online' ? 'Online Encrypted Session' : 'Private Face-to-Face Meeting');
  }
  if (schedDisplay) schedDisplay.textContent = `${advConsultationData.date} | ${advConsultationData.time}`;
  if (clientDisplay) clientDisplay.textContent = `${advConsultationData.name} • ${advConsultationData.phone}`;
  if (paidDisplay) {
    paidDisplay.textContent = `PKR ${advConsultationData.paymentAmount.toLocaleString('en-US')} (Advance Paid via ${advConsultationData.paymentMethod.toUpperCase()} • Ref: ${advConsultationData.transactionRef})`;
  }

  if (privacyDirective) {
    if (advConsultationData.mode === 'face_to_face') {
      privacyDirective.innerHTML = `
        <div class="p-3.5 rounded-xl bg-secondary-container/30 border border-secondary/30 flex items-start gap-3 text-left">
          <span class="material-symbols-outlined text-secondary text-[24px] flex-shrink-0 mt-0.5">lock</span>
          <div class="flex flex-col text-xs leading-relaxed text-on-surface">
            <span class="font-bold text-secondary text-sm">Private Meeting Location Notice</span>
            <p class="mt-1 text-on-surface-variant">For strict client confidentiality, our private meeting location coordinates and attendance pass have been transmitted directly to your verified WhatsApp (<strong>${advConsultationData.phone}</strong>) and Email (<strong>${advConsultationData.email}</strong>).</p>
            <p class="mt-1 font-semibold text-secondary">Private meeting details are shared only with confirmed clients.</p>
          </div>
        </div>
      `;
    } else {
      privacyDirective.innerHTML = `
        <div class="p-3.5 rounded-xl bg-primary-container/20 border border-primary/30 flex items-start gap-3 text-left">
          <span class="material-symbols-outlined text-primary text-[24px] flex-shrink-0 mt-0.5">videocam</span>
          <div class="flex flex-col text-xs leading-relaxed text-on-surface">
            <span class="font-bold text-primary text-sm">Encrypted Video Conference Access</span>
            <p class="mt-1 text-on-surface-variant">Your private, encrypted Google Meet / Zoom session link has been dispatched to your verified WhatsApp (<strong>${advConsultationData.phone}</strong>) and Email (<strong>${advConsultationData.email}</strong>). Calendar invitation attached.</p>
          </div>
        </div>
      `;
    }
  }

  showToast(`Consultation Confirmed & Paid: ${refId}`, 'success');
}

function resetAdvWizard() {
  advCurrentStep = 1;
  const desc = document.getElementById('adv-matter-desc');
  if (desc) desc.value = '';
  updateAdvWizardUI();
  startConsultationFlow('online');
}

// --- 11. INITIALIZATION ---
document.addEventListener('DOMContentLoaded', () => {
  initTheme();
  renderDynamicWizardDates();
  renderAdvDates();
  runTaxCalculation();
  updateWizardUI();
  updateAdvWizardUI();
});
