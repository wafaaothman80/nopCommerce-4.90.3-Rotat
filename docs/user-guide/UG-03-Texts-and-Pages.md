# Texts and Pages

## What it does

Every text on the website can be changed from the admin panel — no developer needed. There are two kinds:

| Kind of text | Where to change it |
|---|---|
| **Content pages**: About Us, Contact Us, Privacy, Conditions, Shipping info, FAQ | Content management → Topics (pages) |
| **Everything else**: button labels, messages, titles anywhere on the site | Configuration → Languages → Edit → String resources |

## Example scenario

Marketing wants the About Us page refreshed and one button renamed. You edit the About Us page under Topics (in all three language tabs), then find the button text under String resources by searching for its current wording, and change it in each language.

## What you need to do

### To edit a content page (e.g., About Us)

1. Go to **Content management → Topics (pages)**.
2. Click **Edit** on the page (e.g., AboutUs).
3. Change the text in the editor. Use the **language tabs** at the top to write the Arabic and French versions.
4. Save. If the site shows the old text, clear the cache (gear icon) and refresh.

> ⚠️ Never **rename** or **delete** these pages — only edit their content. The website finds each page by its name.

### To change any other label or message

1. Copy the exact text you see on the website.
2. Go to **Configuration → Languages** and press **Edit** on the language.
3. Scroll to **String resources** and paste the text into the **Value** search box → Search.
4. Press **Edit** on the found row, change the text, press **Update**.
5. Repeat in the other languages — Arabic, English and French each keep their own texts.

## Good to know

- The site's languages are **AR, EN, FR**. The Arabic language is set to Egypt formatting internally while showing the UAE flag — this is intentional; don't change it.

<figure>
  <img src="/docs-screens/topics-list.png" alt="The content pages list in admin" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Content management → Topics (pages) — the site's content pages.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/language-resources.png" alt="Editing a text in the language resources" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Configuration → Languages → Edit — search any website text by its wording and change it.</figcaption>
</figure>
