/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
document.querySelector('[data-menu-button]')?.addEventListener('click',()=>document.querySelector('[data-menu]')?.classList.toggle('open'));
document.querySelectorAll('[data-image-picker] button').forEach(button=>button.addEventListener('click',()=>{const input=document.querySelector('[data-image-url]');if(input)input.value=button.dataset.image;document.querySelectorAll('[data-image-picker] button').forEach(x=>x.classList.remove('selected'));button.classList.add('selected');}));
