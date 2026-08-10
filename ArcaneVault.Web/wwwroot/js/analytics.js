/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
(function(){
  const d=window.arcaneAnalytics||{};
  initKpiCards();
  if(!window.google||!google.charts)return;
  google.charts.load('current',{packages:['corechart','bar']});
  google.charts.setOnLoadCallback(drawAll);
  const palette=['#2563eb','#7c3aed','#16a34a','#f59e0b','#0891b2','#ec4899'];
  const base={backgroundColor:'transparent',fontName:'Inter',legend:{textStyle:{color:'#374151',fontSize:11}},chartArea:{left:62,top:34,width:'82%',height:'70%'},hAxis:{textStyle:{color:'#4b5563'},gridlines:{color:'transparent'},baselineColor:'#d1d5db'},vAxis:{textStyle:{color:'#4b5563'},gridlines:{color:'#e5e7eb'},baselineColor:'#d1d5db'}};
  // Every empty chart gives the user a recovery action instead of looking broken.
  function empty(id){document.getElementById(id).innerHTML='<div class="chart-empty"><strong>No records match these filters.</strong><span>Reset the filters or choose a wider date range.</span><a href="/Staff/Analytics">Reset filters</a></div>';}
  function drawAll(){drawTrend();drawCategory();drawSources();drawItems();}
  function drawTrend(){
    if(!(d.trend||[]).length)return empty('trend-chart');
    const quantity=d.metric==='quantity',estimated=d.metric==='estimated',bars=d.chartType==='bars';
    const label=quantity?'Items acquired':estimated?'Estimated value of matching items (S$)':'Recorded acquisition value (S$)';
    const current=x=>Number(quantity?(x.secondaryValue||0):estimated?(x.estimatedValue||0):x.value);
    const previous=x=>quantity?x.comparisonSecondaryValue:estimated?x.comparisonEstimatedValue:x.comparisonValue;
    const hasPrevious=d.compare&&d.trend.some(x=>previous(x)!=null);
    const rows=hasPrevious
      ? [["Period","Current period","Previous period"],...d.trend.map(x=>[x.label,current(x),previous(x)==null?null:Number(previous(x))])]
      : [["Period",label],...d.trend.map(x=>[x.label,current(x)])];
    const data=google.visualization.arrayToDataTable(rows);
    const options={...base,legend:{position:hasPrevious?'bottom':'none'},colors:[quantity?'#7c3aed':estimated?'#0891b2':'#2563eb','#9ca3af'],areaOpacity:.10,vAxis:{...base.vAxis,format:quantity?'0':'$#,##0'}};
    const chart=bars?new google.visualization.ColumnChart(document.getElementById('trend-chart')):new google.visualization.AreaChart(document.getElementById('trend-chart'));
    chart.draw(data,options);
  }
  function selected(x){return Number(d.metric==='quantity'?x.value:d.metric==='estimated'?(x.estimatedValue||0):(x.secondaryValue||0));}
  function metricLabel(){return d.metric==='quantity'?'Items acquired':d.metric==='estimated'?'Estimated value of matching items (S$)':'Recorded acquisition value (S$)';}
  function drawCategory(){if(!(d.categories||[]).length)return empty('category-chart');const data=google.visualization.arrayToDataTable([['Category',metricLabel()],...d.categories.map(x=>[x.label,selected(x)])]);new google.visualization.PieChart(document.getElementById('category-chart')).draw(data,{...base,pieHole:.68,pieSliceText:'percentage',pieSliceTextStyle:{fontSize:10},colors:palette,chartArea:{left:18,top:18,width:'94%',height:'76%'},legend:{position:'bottom',textStyle:{fontSize:10,color:'#374151'}}});}
  function drawSources(){if(!(d.sources||[]).length)return empty('source-chart');const data=google.visualization.arrayToDataTable([['Source',metricLabel(),{role:'style'}],...d.sources.map((x,i)=>[x.label,selected(x),palette[i%palette.length]])]);new google.visualization.BarChart(document.getElementById('source-chart')).draw(data,{...base,legend:{position:'none'},chartArea:{left:130,top:20,width:'66%',height:'75%'},hAxis:{minValue:0,format:d.metric==='quantity'?'0':'$#,##0',gridlines:{color:'#e5e7eb'},textStyle:{color:'#4b5563'}}});}
  function drawItems(){if(!(d.items||[]).length)return empty('items-chart');const ranked=[...d.items].sort((a,b)=>selected(b)-selected(a)).slice(0,8);const data=google.visualization.arrayToDataTable([['Item',metricLabel(),{role:'style'},{role:'annotation'}],...ranked.map((x,i)=>[x.label,selected(x),palette[i%palette.length],d.metric==='quantity'?String(selected(x)):'S$'+selected(x).toLocaleString()])]);new google.visualization.ColumnChart(document.getElementById('items-chart')).draw(data,{...base,legend:{position:'none'},vAxis:{...base.vAxis,format:d.metric==='quantity'?'0':'$#,##0'},annotations:{alwaysOutside:true,textStyle:{color:'#111827',fontSize:11}},chartArea:{left:62,top:35,width:'86%',height:'65%'}});}
  // KPI preferences are presentation-only and are kept in this browser; the figures still come from the API.
  function initKpiCards(){
    const grid=document.querySelector('[data-kpi-grid]'),picker=document.querySelector('[data-kpi-picker]');
    if(!grid||!picker)return;
    const defaults=['estimated','recorded','quantity','collectors'];
    const cards=[...grid.querySelectorAll('[data-kpi-card]')],valid=new Set(cards.map(x=>x.dataset.kpiCard));
    const checks=[...picker.querySelectorAll('input[type="checkbox"]')],message=picker.querySelector('[data-kpi-message]');
    let chosen=defaults;
    try{const saved=JSON.parse(localStorage.getItem('arcane-kpi-cards')||'null');if(Array.isArray(saved)){const clean=[...new Set(saved)].filter(x=>valid.has(x)).slice(0,4);if(clean.length)chosen=clean;}}catch{}
    function render(){cards.forEach(x=>x.hidden=true);chosen.forEach(key=>{const card=cards.find(x=>x.dataset.kpiCard===key);if(card){card.hidden=false;grid.appendChild(card);}});checks.forEach(x=>x.checked=chosen.includes(x.value));}
    document.querySelector('[data-kpi-toggle]')?.addEventListener('click',()=>{picker.hidden=!picker.hidden;if(!picker.hidden)message.textContent='';});
    checks.forEach(check=>check.addEventListener('change',()=>{const selected=checks.filter(x=>x.checked);if(selected.length>4){check.checked=false;message.textContent='Choose a maximum of four cards.';}else message.textContent=`${selected.length} of 4 selected`;}));
    picker.querySelector('[data-kpi-apply]')?.addEventListener('click',()=>{const selected=checks.filter(x=>x.checked).map(x=>x.value);if(!selected.length){message.textContent='Choose at least one card.';return;}chosen=selected.slice(0,4);try{localStorage.setItem('arcane-kpi-cards',JSON.stringify(chosen));}catch{}render();picker.hidden=true;});
    picker.querySelector('[data-kpi-reset]')?.addEventListener('click',()=>{chosen=defaults;try{localStorage.removeItem('arcane-kpi-cards');}catch{}render();message.textContent='Default cards restored.';});
    render();
  }
  let resize;window.addEventListener('resize',()=>{clearTimeout(resize);resize=setTimeout(drawAll,150);});
})();
